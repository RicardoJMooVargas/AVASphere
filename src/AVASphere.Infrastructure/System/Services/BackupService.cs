using AVASphere.ApplicationCore.System.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Globalization;

namespace AVASphere.Infrastructure.System.Services;

public class BackupService
{
    private readonly MasterDbContext _dbContext;
    private readonly ILogger<BackupService> _logger;

    public BackupService(MasterDbContext dbContext, ILogger<BackupService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene una lista de todas las tablas disponibles en la base de datos
    /// </summary>
    public async Task<List<string>> GetAvailableTablesAsync()
    {
        try
        {
            _logger.LogInformation("Obteniendo lista de tablas disponibles...");
            
            var tables = new List<string>();
            var connection = _dbContext.Database.GetDbConnection();
            
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT table_name 
                FROM information_schema.tables 
                WHERE table_schema = 'public' 
                  AND table_type = 'BASE TABLE'
                ORDER BY table_name";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }

            _logger.LogInformation($"Se encontraron {tables.Count} tablas disponibles");
            return tables;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de tablas");
            throw new InvalidOperationException($"Error al obtener tablas: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Exporta todas las tablas de la base de datos como consultas SQL INSERT
    /// </summary>
    public async Task<string> ExportAllTablesAsSqlAsync()
    {
        try
        {
            _logger.LogInformation("Iniciando exportación completa de la base de datos...");
            
            var sql = new StringBuilder();
            var tables = await GetAvailableTablesAsync();
            var dependencies = await GetTableDependenciesAsync();
            
            // Ordenar tablas por dependencias (las tablas referenciadas primero)
            var orderedTables = OrderTablesByDependencies(tables, dependencies);
            
            sql.AppendLine("-- BACKUP COMPLETO DE LA BASE DE DATOS");
            sql.AppendLine($"-- Generado el: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sql.AppendLine($"-- Total de tablas: {tables.Count}");
            sql.AppendLine($"-- Orden de exportación respeta dependencias de clave foránea");
            sql.AppendLine();

            foreach (var tableName in orderedTables)
            {
                try
                {
                    var tableData = await ExportTableAsSqlAsync(tableName);
                    sql.AppendLine($"-- TABLA: {tableName}");
                    sql.AppendLine(tableData);
                    sql.AppendLine();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al exportar tabla {TableName}", tableName);
                    sql.AppendLine($"-- ERROR al exportar tabla {tableName}: {ex.Message}");
                    sql.AppendLine();
                }
            }

            _logger.LogInformation("Exportación completa finalizada");
            return sql.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en exportación completa");
            throw new InvalidOperationException($"Error en exportación: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Exporta tablas específicas como consultas SQL INSERT
    /// </summary>
    public async Task<string> ExportSelectedTablesAsSqlAsync(List<string> tableNames)
    {
        try
        {
            _logger.LogInformation($"Iniciando exportación de {tableNames.Count} tablas seleccionadas...");
            
            var sql = new StringBuilder();
            var availableTables = await GetAvailableTablesAsync();
            var dependencies = await GetTableDependenciesAsync();
            
            // Filtrar solo tablas que existen
            var validTables = tableNames.Where(t => availableTables.Contains(t, StringComparer.OrdinalIgnoreCase)).ToList();
            
            // Ordenar tablas seleccionadas por dependencias
            var orderedTables = OrderTablesByDependencies(validTables, dependencies);
            
            sql.AppendLine("-- BACKUP DE TABLAS SELECCIONADAS");
            sql.AppendLine($"-- Generado el: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sql.AppendLine($"-- Tablas solicitadas: {string.Join(", ", tableNames)}");
            sql.AppendLine($"-- Tablas válidas encontradas: {string.Join(", ", validTables)}");
            sql.AppendLine($"-- Orden de exportación: {string.Join(" -> ", orderedTables)}");
            sql.AppendLine();

            foreach (var tableName in orderedTables)
            {
                try
                {
                    var tableData = await ExportTableAsSqlAsync(tableName);
                    sql.AppendLine($"-- TABLA: {tableName}");
                    sql.AppendLine(tableData);
                    sql.AppendLine();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al exportar tabla {TableName}", tableName);
                    sql.AppendLine($"-- ERROR al exportar tabla {tableName}: {ex.Message}");
                    sql.AppendLine();
                }
            }
            
            // Reportar tablas no encontradas
            var missingTables = tableNames.Except(validTables, StringComparer.OrdinalIgnoreCase).ToList();
            foreach (var missingTable in missingTables)
            {
                _logger.LogWarning("Tabla {TableName} no encontrada en la base de datos", missingTable);
                sql.AppendLine($"-- ADVERTENCIA: Tabla {missingTable} no encontrada");
            }

            _logger.LogInformation("Exportación de tablas seleccionadas finalizada");
            return sql.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en exportación de tablas seleccionadas");
            throw new InvalidOperationException($"Error en exportación: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Importa datos desde consultas SQL con manejo de errores y reglas específicas
    /// </summary>
    public async Task<BackupImportResultDto> ImportFromSqlAsync(string sqlQueries, bool overwrite = false)
    {
        var result = new BackupImportResultDto
        {
            Success = false,
            OverwriteMode = overwrite,
            ExecutedStatements = new List<string>(),
            SkippedStatements = new List<string>(),
            Errors = new List<string>(),
            Warnings = new List<string>()
        };

        try
        {
            _logger.LogInformation($"Iniciando importación SQL (overwrite: {overwrite})...");
            
            var statements = ParseSqlStatements(sqlQueries);
            var availableTables = await GetAvailableTablesAsync();
            var dependencies = await GetTableDependenciesAsync();
            
            // Ordenar declaraciones por dependencias de tabla
            var orderedStatements = OrderStatementsByTableDependencies(statements, dependencies);
            
            _logger.LogInformation($"Procesando {orderedStatements.Count} declaraciones SQL ordenadas por dependencias");
            
            // Procesar cada declaración por separado SIN transacción global
            // Esto evita que un error aborte toda la operación
            foreach (var statement in orderedStatements)
            {
                await ProcessSqlStatementIndividuallyAsync(statement, availableTables, overwrite, result);
            }

            // Considerar exitoso si al menos el 50% de las declaraciones se ejecutaron
            var successRate = orderedStatements.Count > 0 ? (double)result.ExecutedStatements.Count / orderedStatements.Count : 0;
            result.Success = successRate >= 0.5 || result.Errors.Count == 0;
            
            result.Message = $"Importación completada. Ejecutadas: {result.ExecutedStatements.Count}, " +
                           $"Omitidas: {result.SkippedStatements.Count}, Errores: {result.Errors.Count}";
            
            _logger.LogInformation(result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en importación SQL");
            result.Success = false;
            result.Message = $"Error en importación: {ex.Message}";
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    #region Private Helper Methods

    /// <summary>
    /// Exporta una tabla específica como consultas SQL INSERT
    /// </summary>
    private async Task<string> ExportTableAsSqlAsync(string tableName)
    {
        var sql = new StringBuilder();
        var connection = _dbContext.Database.GetDbConnection();
        
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        // Obtener estructura de la tabla
        var columns = await GetTableColumnsAsync(tableName, connection);
        if (!columns.Any())
        {
            return $"-- Tabla {tableName} no tiene columnas o no existe";
        }

        // Obtener datos
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM \"{tableName}\"";
        
        using var reader = await command.ExecuteReaderAsync();
        var insertStatements = new List<string>();
        
        while (await reader.ReadAsync())
        {
            var values = new List<string>();
            
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var value = reader.GetValue(i);
                values.Add(FormatSqlValue(value));
            }
            
            var columnNames = string.Join(",", columns.Select(c => $"\"{c}\""));
            var valuesList = string.Join(",", values);
            insertStatements.Add($"INSERT INTO \"{tableName}\"({columnNames}) VALUES({valuesList});");
        }
        
        if (insertStatements.Any())
        {
            sql.AppendLine($"-- Datos para tabla {tableName} ({insertStatements.Count} registros)");
            foreach (var statement in insertStatements)
            {
                sql.AppendLine(statement);
            }
        }
        else
        {
            sql.AppendLine($"-- Tabla {tableName} está vacía");
        }
        
        return sql.ToString();
    }

    /// <summary>
    /// Obtiene las columnas de una tabla
    /// </summary>
    private async Task<List<string>> GetTableColumnsAsync(string tableName, IDbConnection connection)
    {
        var columns = new List<string>();
        
        using var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT column_name 
            FROM information_schema.columns 
            WHERE table_name = '{tableName}' 
              AND table_schema = 'public'
            ORDER BY ordinal_position";
        
        var reader = await ((DbCommand)command).ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(reader.GetString(0));
        }
        
        reader.Close();
        return columns;
    }

    /// <summary>
    /// Formatea un valor para inserción SQL, manejando tipos especiales
    /// </summary>
    private string FormatSqlValue(object value)
    {
        if (value == null || value == DBNull.Value)
            return "NULL";
        
        return value switch
        {
            string s => $"'{s.Replace("'", "''")}'", // Escapar comillas simples
            DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
            DateOnly d => $"'{d:yyyy-MM-dd}'",
            TimeOnly t => $"'{t:HH:mm:ss}'",
            bool b => b ? "true" : "false",
            byte[] bytes => $"'\\x{Convert.ToHexString(bytes)}'",
            decimal dec => dec.ToString("0.##########", CultureInfo.InvariantCulture),
            double dbl => dbl.ToString("0.##########", CultureInfo.InvariantCulture),
            float flt => flt.ToString("0.##########", CultureInfo.InvariantCulture),
            Guid guid => $"'{guid}'",
            // Para tipos numéricos, usar formato invariante
            int or long or short or byte => value.ToString(),
            // Para cualquier otro tipo, convertir a string y escapar
            _ => $"'{value.ToString()?.Replace("'", "''")}'"
        };
    }

    /// <summary>
    /// Parsea las declaraciones SQL del texto de entrada
    /// </summary>
    private List<string> ParseSqlStatements(string sqlQueries)
    {
        var statements = new List<string>();
        var lines = sqlQueries.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var currentStatement = new StringBuilder();
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Ignorar comentarios
            if (trimmedLine.StartsWith("--") || string.IsNullOrWhiteSpace(trimmedLine))
                continue;
            
            currentStatement.AppendLine(line);
            
            // Si la línea termina con punto y coma, es el final de una declaración
            if (trimmedLine.EndsWith(";"))
            {
                statements.Add(currentStatement.ToString().Trim());
                currentStatement.Clear();
            }
        }
        
        // Agregar cualquier declaración restante
        if (currentStatement.Length > 0)
        {
            statements.Add(currentStatement.ToString().Trim());
        }
        
        return statements;
    }

    /// <summary>
    /// Procesa una declaración SQL individual con su propia transacción
    /// </summary>
    private async Task ProcessSqlStatementIndividuallyAsync(string statement, List<string> availableTables, 
        bool overwrite, BackupImportResultDto result)
    {
        try
        {
            // Extraer nombre de tabla del INSERT antes de la limpieza para logging
            var tableName = ExtractTableNameFromInsert(statement);
            
            _logger.LogDebug("Procesando declaración para tabla: {TableName}", tableName);
            
            // Validar y limpiar la declaración SQL
            var originalStatement = statement;
            statement = ValidateAndCleanSqlStatement(statement);
            
            if (statement != originalStatement)
            {
                _logger.LogDebug("Declaración SQL limpiada para tabla {TableName}. Original length: {OriginalLength}, New length: {NewLength}", 
                    tableName, originalStatement.Length, statement.Length);
            }
            
            if (string.IsNullOrEmpty(tableName))
            {
                result.SkippedStatements.Add($"No se pudo identificar tabla: {statement.Substring(0, Math.Min(50, statement.Length))}...");
                return;
            }

            // REGLA 1: Si la tabla no existe en la DB, omitir
            if (!availableTables.Contains(tableName, StringComparer.OrdinalIgnoreCase))
            {
                result.SkippedStatements.Add($"Tabla '{tableName}' no existe en la base de datos");
                result.Warnings.Add($"Tabla '{tableName}' omitida - no existe en la DB actual");
                return;
            }

            // REGLA 2: Validar columnas y ajustar declaración
            var adjustedStatement = await AdjustStatementForMissingColumnsAsync(statement, tableName);
            if (adjustedStatement != statement)
            {
                result.Warnings.Add($"Declaración ajustada para tabla '{tableName}' - columnas faltantes omitidas");
                _logger.LogDebug("Declaración ajustada para tabla {TableName} por columnas faltantes", tableName);
            }

            // Usar una transacción individual para esta declaración
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            
            try
            {
                _logger.LogDebug("Ejecutando INSERT para tabla {TableName}", tableName);
                
                // Intentar ejecutar primero como INSERT normal
                await _dbContext.Database.ExecuteSqlRawAsync(adjustedStatement);
                await transaction.CommitAsync();
                result.ExecutedStatements.Add($"Tabla '{tableName}': INSERT ejecutado exitosamente");
                
                _logger.LogDebug("INSERT exitoso para tabla {TableName}", tableName);
            }
            catch (Exception ex) when (IsDuplicateKeyError(ex))
            {
                await transaction.RollbackAsync();
                
                _logger.LogDebug("Conflicto de clave duplicada en tabla {TableName}, overwrite: {Overwrite}", tableName, overwrite);
                
                // Si hay conflicto de clave y overwrite está activado, usar UPSERT
                if (overwrite)
                {
                    using var upsertTransaction = await _dbContext.Database.BeginTransactionAsync();
                    try
                    {
                        var upsertStatement = ConvertToUpsert(adjustedStatement, tableName);
                        _logger.LogDebug("Ejecutando UPSERT para tabla {TableName}", tableName);
                        
                        await _dbContext.Database.ExecuteSqlRawAsync(upsertStatement);
                        await upsertTransaction.CommitAsync();
                        result.ExecutedStatements.Add($"Tabla '{tableName}': UPSERT ejecutado exitosamente (overwrite)");
                        
                        _logger.LogDebug("UPSERT exitoso para tabla {TableName}", tableName);
                    }
                    catch (Exception upsertEx)
                    {
                        await upsertTransaction.RollbackAsync();
                        var upsertError = $"Error en UPSERT para tabla '{tableName}': {upsertEx.Message}";
                        result.Errors.Add(upsertError);
                        result.SkippedStatements.Add(statement);
                        
                        _logger.LogWarning("Error en UPSERT para tabla {TableName}: {Error}", tableName, upsertEx.Message);
                    }
                }
                else
                {
                    result.SkippedStatements.Add($"Tabla '{tableName}': INSERT omitido - registro duplicado (overwrite=false)");
                    result.Warnings.Add($"Registro duplicado en tabla '{tableName}' omitido porque overwrite=false");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                
                var errorMsg = $"Error ejecutando en tabla '{tableName}': {ex.Message}";
                result.Errors.Add(errorMsg);
                result.SkippedStatements.Add(statement);
                
                // Logging detallado del error para debugging
                _logger.LogWarning("Error ejecutando SQL para tabla {TableName}: {Error}. Statement length: {Length}", 
                    tableName, ex.Message, adjustedStatement.Length);
                
                // Si es error de formato, loggear parte de la declaración para debug
                if (ex.Message.Contains("Input string was not in a correct format"))
                {
                    _logger.LogWarning("Declaración problemática (primeros 200 chars): {Statement}", 
                        adjustedStatement.Substring(0, Math.Min(200, adjustedStatement.Length)));
                }
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error procesando declaración: {ex.Message}";
            result.Errors.Add(errorMsg);
            result.SkippedStatements.Add(statement);
            _logger.LogWarning(ex, "Error al procesar declaración SQL");
        }
    }

    /// <summary>
    /// Determina si un error es de clave duplicada
    /// </summary>
    private bool IsDuplicateKeyError(Exception ex)
    {
        return ex.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("23505") ||
               ex.Message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Extrae el nombre de tabla de una declaración INSERT
    /// </summary>
    private string ExtractTableNameFromInsert(string statement)
    {
        var match = Regex.Match(
            statement, @"INSERT\s+INTO\s+[""']?([^""'\s\(]+)[""']?", 
            RegexOptions.IgnoreCase);
        
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    /// <summary>
    /// Ajusta una declaración SQL para omitir columnas que no existen en la tabla actual
    /// </summary>
    private async Task<string> AdjustStatementForMissingColumnsAsync(string statement, string tableName)
    {
        try
        {
            var connection = _dbContext.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();
                
            var existingColumns = await GetTableColumnsAsync(tableName, connection);
            
            // Parsear la declaración INSERT para extraer columnas y valores
            var insertMatch = Regex.Match(
                statement, @"INSERT\s+INTO\s+[""']?([^""'\s\(]+)[""']?\s*\(([^)]+)\)\s*VALUES\s*\(([^)]+)\)",
                RegexOptions.IgnoreCase);
            
            if (!insertMatch.Success)
                return statement; // No se pudo parsear, devolver original
            
            var columns = insertMatch.Groups[2].Value
                .Split(',')
                .Select(c => c.Trim().Trim('"', '\''))
                .ToList();
            
            var values = insertMatch.Groups[3].Value
                .Split(',')
                .Select(v => v.Trim())
                .ToList();
            
            if (columns.Count != values.Count)
                return statement; // Inconsistencia, devolver original
            
            // Filtrar solo columnas que existen en la tabla actual
            var validColumns = new List<string>();
            var validValues = new List<string>();
            
            for (int i = 0; i < columns.Count; i++)
            {
                if (existingColumns.Contains(columns[i], StringComparer.OrdinalIgnoreCase))
                {
                    validColumns.Add("\"" + columns[i] + "\"");
                    validValues.Add(values[i]);
                }
            }
            
            if (validColumns.Count == 0)
                throw new InvalidOperationException($"No hay columnas válidas para la tabla {tableName}");
            
            // Reconstruir la declaración
            var newStatement = $"INSERT INTO \"{tableName}\"({string.Join(",", validColumns)}) VALUES({string.Join(",", validValues)})";
            
            return newStatement;
        }
        catch (Exception)
        {
            // Si hay error en el ajuste, devolver declaración original
            return statement;
        }
    }

    /// <summary>
    /// Convierte un INSERT en UPSERT para PostgreSQL
    /// </summary>
    private string ConvertToUpsert(string insertStatement, string tableName)
    {
        try
        {
            // Para PostgreSQL, usar ON CONFLICT DO UPDATE
            var primaryKey = GetPrimaryKeyName(tableName);
            
            if (string.IsNullOrEmpty(primaryKey))
                return insertStatement; // No se pudo determinar PK, usar INSERT normal
            
            // Extraer las columnas del INSERT
            var insertMatch = Regex.Match(
                insertStatement, @"INSERT\s+INTO\s+[""']?([^""'\s\(]+)[""']?\s*\(([^)]+)\)\s*VALUES\s*\(([^)]+)\)",
                RegexOptions.IgnoreCase);
            
            if (!insertMatch.Success)
                return insertStatement; // No se pudo parsear, devolver original
            
            var columns = insertMatch.Groups[2].Value
                .Split(',')
                .Select(c => c.Trim().Trim('"', '\''))
                .Where(c => !string.IsNullOrEmpty(c))
                .ToList();
            
            var values = insertMatch.Groups[3].Value
                .Split(',')
                .Select(v => v.Trim())
                .ToList();
            
            if (columns.Count != values.Count)
                return insertStatement; // Inconsistencia, devolver original
            
            // Construir el UPSERT
            var updateColumns = new List<string>();
            for (int i = 0; i < columns.Count; i++)
            {
                if (!columns[i].Equals(primaryKey, StringComparison.OrdinalIgnoreCase))
                {
                    updateColumns.Add($"\"{columns[i]}\" = EXCLUDED.\"{columns[i]}\"");
                }
            }
            
            var upsertStatement = insertStatement.TrimEnd(';') + 
                $" ON CONFLICT (\"{primaryKey}\") DO UPDATE SET " +
                string.Join(", ", updateColumns) + ";";
            
            return upsertStatement;
        }
        catch (Exception)
        {
            // Si falla el UPSERT, devolver INSERT original
            return insertStatement;
        }
    }

    /// <summary>
    /// Obtiene el nombre de la clave primaria de una tabla
    /// </summary>
    private string GetPrimaryKeyName(string tableName)
    {
        // Mapeo simplificado de nombres de tabla a clave primaria
        return tableName.ToLowerInvariant() switch
        {
            "property" => "IdProperty",
            "propertyvalue" => "IdPropertyValue", 
            "supplier" => "IdSupplier",
            "warehouse" => "IdWarehouse",
            "storagestructure" => "IdStorageStructure",
            "area" => "IdArea",
            "user" => "IdUser",
            "rol" => "IdRol",
            "configsys" => "IdConfigSys",
            "customer" => "IdCustomer",
            _ => $"Id{tableName}" // Convención por defecto
        };
    }

    /// <summary>
    /// Obtiene las columnas para el UPDATE en UPSERT
    /// </summary>
    private List<string> GetUpdateColumnsForUpsert(string insertStatement, string primaryKey)
    {
        var updateColumns = new List<string>();
        
        try
        {
            var match = Regex.Match(
                insertStatement, @"\(([^)]+)\)", 
                RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                var columns = match.Groups[1].Value
                    .Split(',')
                    .Select(c => c.Trim().Trim('"', '\''))
                    .Where(c => !c.Equals(primaryKey, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                foreach (var column in columns)
                {
                    updateColumns.Add("\"" + column + "\" = EXCLUDED.\"" + column + "\"");
                }
            }
        }
        catch (Exception)
        {
            // En caso de error, devolver lista vacía
        }
        
        return updateColumns;
    }

    /// <summary>
    /// Valida y limpia una declaración SQL para evitar errores de formato
    /// </summary>
    private string ValidateAndCleanSqlStatement(string statement)
    {
        if (string.IsNullOrWhiteSpace(statement))
            return statement;
        
        try
        {
            // Limpiar espacios extra y saltos de línea
            statement = statement.Trim();
            
            // Remover múltiples espacios consecutivos
            statement = Regex.Replace(statement, @"\s+", " ");
            
            // LIMPIEZA ESPECÍFICA POR TABLA
            var tableName = ExtractTableNameFromInsert(statement);
            if (!string.IsNullOrEmpty(tableName))
            {
                switch (tableName.ToLowerInvariant())
                {
                    case "product":
                        statement = CleanProductInsertStatement(statement);
                        break;
                    case "supplier":
                        statement = CleanSupplierInsertStatement(statement);
                        break;
                    default:
                        // Para otras tablas, usar limpieza general
                        statement = EscapeProblematicCharacters(statement);
                        break;
                }
            }
            else
            {
                // Si no se puede identificar la tabla, usar limpieza general
                statement = EscapeProblematicCharacters(statement);
            }
            
            // Asegurar que termine con punto y coma
            if (!statement.EndsWith(";"))
                statement += ";";
            
            // Validar que sea una declaración INSERT válida
            if (!Regex.IsMatch(statement, @"^\s*INSERT\s+INTO\s+", RegexOptions.IgnoreCase))
            {
                throw new InvalidOperationException("La declaración no es un INSERT válido");
            }
            
            return statement;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error validando declaración SQL: {ex.Message}. Declaración: {statement.Substring(0, Math.Min(100, statement.Length))}...");
            return statement; // Devolver original si hay error en la validación
        }
    }

    /// <summary>
    /// Escapa caracteres problemáticos en declaraciones SQL
    /// </summary>
    private string EscapeProblematicCharacters(string statement)
    {
        // Buscar VALUES y procesar los valores entre paréntesis
        var match = Regex.Match(statement, @"VALUES\s*\(([^)]+)\)", RegexOptions.IgnoreCase);
        if (!match.Success)
            return statement;

        var valuesString = match.Groups[1].Value;
        var values = SplitSqlValues(valuesString);
        var cleanedValues = new List<string>();

        foreach (var value in values)
        {
            var cleanValue = CleanSqlValue(value.Trim());
            cleanedValues.Add(cleanValue);
        }

        var newValuesString = string.Join(",", cleanedValues);
        return statement.Replace(match.Groups[1].Value, newValuesString);
    }

    /// <summary>
    /// Divide los valores SQL respetando las comillas y estructuras JSON anidadas
    /// </summary>
    private List<string> SplitSqlValues(string valuesString)
    {
        var values = new List<string>();
        var currentValue = new StringBuilder();
        bool inQuotes = false;
        char quoteChar = '\0';
        int bracketDepth = 0;
        int braceDepth = 0;

        for (int i = 0; i < valuesString.Length; i++)
        {
            char c = valuesString[i];

            if (!inQuotes && (c == '\'' || c == '"'))
            {
                inQuotes = true;
                quoteChar = c;
                currentValue.Append(c);
            }
            else if (inQuotes && c == quoteChar)
            {
                // Verificar si es una comilla escapada
                if (i + 1 < valuesString.Length && valuesString[i + 1] == quoteChar)
                {
                    // Es una comilla escapada, agregar ambas
                    currentValue.Append(c);
                    currentValue.Append(valuesString[i + 1]);
                    i++; // Saltar el siguiente carácter
                }
                else
                {
                    // Es el final de las comillas
                    inQuotes = false;
                    currentValue.Append(c);
                }
            }
            else if (inQuotes)
            {
                // Dentro de comillas, agregar todo tal como está
                currentValue.Append(c);
                
                // Rastrear profundidad de estructuras JSON dentro de comillas
                if (c == '[')
                    bracketDepth++;
                else if (c == ']')
                    bracketDepth--;
                else if (c == '{')
                    braceDepth++;
                else if (c == '}')
                    braceDepth--;
            }
            else if (!inQuotes && c == ',' && bracketDepth == 0 && braceDepth == 0)
            {
                // Solo dividir por coma si no estamos dentro de comillas o estructuras JSON
                values.Add(currentValue.ToString().Trim());
                currentValue.Clear();
            }
            else
            {
                // Fuera de comillas
                currentValue.Append(c);
                
                // Rastrear estructuras JSON fuera de comillas (aunque debería ser raro)
                if (c == '[')
                    bracketDepth++;
                else if (c == ']')
                    bracketDepth--;
                else if (c == '{')
                    braceDepth++;
                else if (c == '}')
                    braceDepth--;
            }
        }

        if (currentValue.Length > 0)
        {
            values.Add(currentValue.ToString().Trim());
        }

        return values;
    }

    /// <summary>
    /// Limpia un valor SQL individual
    /// </summary>
    private string CleanSqlValue(string value)
    {
        value = value.Trim();

        // Si es NULL, devolver tal como está
        if (string.Equals(value, "NULL", StringComparison.OrdinalIgnoreCase))
            return "NULL";

        // Si es un número, validar formato
        if (Regex.IsMatch(value, @"^-?\d+(\.\d+)?$"))
        {
            return value;
        }

        // Si es boolean
        if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
        {
            return value.ToLower();
        }

        // Si es una fecha/timestamp
        if (Regex.IsMatch(value, @"^\d{4}-\d{2}-\d{2}"))
        {
            // Asegurar que esté entre comillas
            if (!value.StartsWith("'"))
                value = "'" + value;
            if (!value.EndsWith("'"))
                value = value + "'";
            return value;
        }

        // MANEJO ESPECIAL PARA VALORES JSON
        // Detectar si es un valor JSON (comienza con [ o {)
        if (IsJsonValue(value))
        {
            return CleanJsonValue(value);
        }

        // Si es un string, asegurar que esté entre comillas y escapar contenido
        if (value.StartsWith("'") && value.EndsWith("'"))
        {
            // Ya está entre comillas, solo escapar contenido interno
            var content = value.Substring(1, value.Length - 2);
            content = EscapeStringContent(content);
            return "'" + content + "'";
        }

        // Si no está entre comillas pero parece ser un string, agregárselas
        if (!value.StartsWith("'"))
        {
            value = EscapeStringContent(value);
            return "'" + value + "'";
        }

        return value;
    }

    /// <summary>
    /// Determina si un valor es JSON
    /// </summary>
    private bool IsJsonValue(string value)
    {
        value = value.Trim();
        
        // Remover comillas externas si existen
        if (value.StartsWith("'") && value.EndsWith("'"))
        {
            value = value.Substring(1, value.Length - 2);
        }
        
        return value.StartsWith("[") || value.StartsWith("{");
    }

    /// <summary>
    /// Limpia valores JSON para PostgreSQL
    /// </summary>
    private string CleanJsonValue(string jsonValue)
    {
        try
        {
            // Remover comillas externas si existen
            var content = jsonValue.Trim();
            if (content.StartsWith("'") && content.EndsWith("'"))
            {
                content = content.Substring(1, content.Length - 2);
            }

            // Para PostgreSQL, los JSON se almacenan como strings
            // Necesitamos escapar las comillas internas
            content = content.Replace("'", "''");
            
            return "'" + content + "'";
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error limpiando valor JSON: {ex.Message}. Valor: {jsonValue.Substring(0, Math.Min(100, jsonValue.Length))}");
            
            // Fallback: tratar como string normal
            var escaped = jsonValue.Replace("'", "''");
            return "'" + escaped + "'";
        }
    }

    /// <summary>
    /// Escapa contenido de string para PostgreSQL
    /// </summary>
    private string EscapeStringContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return content;

        // Escapar comillas simples
        content = content.Replace("'", "''");
        
        // Escapar caracteres de escape de barra invertida si es necesario
        // PostgreSQL usa \' para escapar, pero nosotros usamos '' que es más seguro
        
        return content;
    }

    /// <summary>
    /// Limpia específicamente declaraciones INSERT de la tabla Product
    /// </summary>
    private string CleanProductInsertStatement(string statement)
    {
        try
        {
            // Patrón específico para tabla Product que tiene campos JSON
            var match = Regex.Match(statement, 
                @"INSERT\s+INTO\s+""Product""\s*\([^)]+\)\s*VALUES\s*\(([^)]+)\)", 
                RegexOptions.IgnoreCase);
            
            if (!match.Success)
                return statement;

            var valuesString = match.Groups[1].Value;
            
            // Para Product, esperamos estos campos en orden:
            // IdProduct, MainName, Unit, Description, Quantity, Taxes, IdSupplier, CodeJson, CostsJson, CategoriesJsons, SolutionsJsons
            
            var cleanedValues = CleanProductValues(valuesString);
            var newStatement = statement.Replace(match.Groups[1].Value, cleanedValues);
            
            _logger.LogDebug("Declaración Product limpiada. Original: {Original}, Nueva: {New}", 
                valuesString.Substring(0, Math.Min(100, valuesString.Length)), 
                cleanedValues.Substring(0, Math.Min(100, cleanedValues.Length)));
            
            return newStatement;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error limpiando declaración Product: {Error}", ex.Message);
            return statement; // Fallback al original
        }
    }

    /// <summary>
    /// Limpia específicamente los valores de la tabla Product
    /// </summary>
    private string CleanProductValues(string valuesString)
    {
        // Dividir manualmente los valores de Product considerando la estructura esperada
        var values = new List<string>();
        var currentValue = new StringBuilder();
        bool inQuotes = false;
        int commaCount = 0;
        
        for (int i = 0; i < valuesString.Length; i++)
        {
            char c = valuesString[i];
            
            if (c == '\'' && !inQuotes)
            {
                inQuotes = true;
                currentValue.Append(c);
            }
            else if (c == '\'' && inQuotes)
            {
                // Verificar si es comilla escapada
                if (i + 1 < valuesString.Length && valuesString[i + 1] == '\'')
                {
                    currentValue.Append("''"); // Mantener escape
                    i++; // Skip next quote
                }
                else
                {
                    inQuotes = false;
                    currentValue.Append(c);
                }
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(CleanProductValue(currentValue.ToString().Trim(), commaCount));
                currentValue.Clear();
                commaCount++;
            }
            else
            {
                currentValue.Append(c);
            }
        }
        
        // Agregar el último valor
        if (currentValue.Length > 0)
        {
            values.Add(CleanProductValue(currentValue.ToString().Trim(), commaCount));
        }
        
        return string.Join(",", values);
    }

    /// <summary>
    /// Limpia un valor específico de Product basado en su posición
    /// </summary>
    private string CleanProductValue(string value, int position)
    {
        value = value.Trim();
        
        // Posiciones de campos Product:
        // 0: IdProduct (int)
        // 1: MainName (string)  
        // 2: Unit (string)
        // 3: Description (string)
        // 4: Quantity (decimal)
        // 5: Taxes (decimal)
        // 6: IdSupplier (int)
        // 7: CodeJson (json)
        // 8: CostsJson (json)
        // 9: CategoriesJsons (json)
        // 10: SolutionsJsons (json)
        
        switch (position)
        {
            case 0: // IdProduct
            case 4: // Quantity  
            case 5: // Taxes
            case 6: // IdSupplier
                // Campos numéricos
                return Regex.IsMatch(value, @"^-?\d+(\.\d+)?$") ? value : "0";
                
            case 1: // MainName
            case 2: // Unit
            case 3: // Description
                // Campos string normales
                return CleanSimpleStringValue(value);
                
            case 7: // CodeJson
            case 8: // CostsJson
            case 9: // CategoriesJsons
            case 10: // SolutionsJsons
                // Campos JSON
                return CleanJsonFieldValue(value);
                
            default:
                return CleanSqlValue(value);
        }
    }

    /// <summary>
    /// Limpia específicamente declaraciones INSERT de la tabla Supplier
    /// </summary>
    private string CleanSupplierInsertStatement(string statement)
    {
        try
        {
            // Patrón específico para tabla Supplier que tiene campos JSON y double
            var match = Regex.Match(statement, 
                @"INSERT\s+INTO\s+""Supplier""\s*\([^)]+\)\s*VALUES\s*\(([^)]+)\)", 
                RegexOptions.IgnoreCase);
            
            if (!match.Success)
                return statement;

            var valuesString = match.Groups[1].Value;
            
            // Para Supplier, esperamos estos campos en orden:
            // IdSupplier, Name, CompanyName, TaxId, PersonType, BusinessId, CurrencyCoin, DeliveryDays, RegistrationDate, Observations, ContactsJson, PaymentTermsJson, PaymentMethodsJson
            
            var cleanedValues = CleanSupplierValues(valuesString);
            var newStatement = statement.Replace(match.Groups[1].Value, cleanedValues);
            
            _logger.LogDebug("Declaración Supplier limpiada. Original: {Original}, Nueva: {New}", 
                valuesString.Substring(0, Math.Min(100, valuesString.Length)), 
                cleanedValues.Substring(0, Math.Min(100, cleanedValues.Length)));
            
            return newStatement;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error limpiando declaración Supplier: {Error}", ex.Message);
            return statement; // Fallback al original
        }
    }

    /// <summary>
    /// Limpia específicamente los valores de la tabla Supplier
    /// </summary>
    private string CleanSupplierValues(string valuesString)
    {
        // Dividir manualmente los valores de Supplier considerando la estructura esperada
        var values = new List<string>();
        var currentValue = new StringBuilder();
        bool inQuotes = false;
        int commaCount = 0;
        
        for (int i = 0; i < valuesString.Length; i++)
        {
            char c = valuesString[i];
            
            if (c == '\'' && !inQuotes)
            {
                inQuotes = true;
                currentValue.Append(c);
            }
            else if (c == '\'' && inQuotes)
            {
                // Verificar si es comilla escapada
                if (i + 1 < valuesString.Length && valuesString[i + 1] == '\'')
                {
                    currentValue.Append("''"); // Mantener escape
                    i++; // Skip next quote
                }
                else
                {
                    inQuotes = false;
                    currentValue.Append(c);
                }
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(CleanSupplierValue(currentValue.ToString().Trim(), commaCount));
                currentValue.Clear();
                commaCount++;
            }
            else
            {
                currentValue.Append(c);
            }
        }
        
        // Agregar el último valor
        if (currentValue.Length > 0)
        {
            values.Add(CleanSupplierValue(currentValue.ToString().Trim(), commaCount));
        }
        
        return string.Join(",", values);
    }

    /// <summary>
    /// Limpia un valor específico de Supplier basado en su posición
    /// </summary>
    private string CleanSupplierValue(string value, int position)
    {
        value = value.Trim();
        
        // Posiciones de campos Supplier:
        // 0: IdSupplier (int)
        // 1: Name (string)  
        // 2: CompanyName (string)
        // 3: TaxId (string)
        // 4: PersonType (string)
        // 5: BusinessId (string)
        // 6: CurrencyCoin (string)
        // 7: DeliveryDays (double)
        // 8: RegistrationDate (DateTime)
        // 9: Observations (string)
        // 10: ContactsJson (json)
        // 11: PaymentTermsJson (json)
        // 12: PaymentMethodsJson (json)
        
        switch (position)
        {
            case 0: // IdSupplier
                // Campo numérico entero
                return Regex.IsMatch(value, @"^-?\d+$") ? value : "0";
                
            case 1: // Name
            case 2: // CompanyName
            case 3: // TaxId
            case 4: // PersonType
            case 5: // BusinessId
            case 6: // CurrencyCoin
            case 9: // Observations
                // Campos string normales
                return CleanSimpleStringValue(value);
                
            case 7: // DeliveryDays (double)
                // Campo numérico decimal
                return Regex.IsMatch(value, @"^-?\d+(\.\d+)?$") ? value : "0";
                
            case 8: // RegistrationDate (DateTime)
                // Campo DateTime
                return CleanDateTimeValue(value);
                
            case 10: // ContactsJson
            case 11: // PaymentTermsJson
            case 12: // PaymentMethodsJson
                // Campos JSON
                return CleanJsonFieldValue(value);
                
            default:
                return CleanSqlValue(value);
        }
    }

    /// <summary>
    /// Limpia valores DateTime específicamente
    /// </summary>
    private string CleanDateTimeValue(string value)
    {
        value = value.Trim();
        
        // Si es NULL
        if (string.Equals(value, "NULL", StringComparison.OrdinalIgnoreCase))
            return "NULL";
        
        // Si parece una fecha
        if (Regex.IsMatch(value, @"^\d{4}-\d{2}-\d{2}"))
        {
            // Remover comillas externas si existen
            if (value.StartsWith("'") && value.EndsWith("'"))
            {
                return value; // Ya está bien formateado
            }
            return "'" + value + "'";
        }
        
        // Fallback para fechas malformadas
        return "'1900-01-01 00:00:00'";
    }

    /// <summary>
    /// Limpia un campo JSON específico de tablas como Product y Supplier
    /// </summary>
    private string CleanJsonFieldValue(string value)
    {
        value = value.Trim();
        
        // Si es NULL
        if (string.Equals(value, "NULL", StringComparison.OrdinalIgnoreCase))
            return "NULL";
        
        // Remover comillas externas si existen
        if (value.StartsWith("'") && value.EndsWith("'"))
        {
            value = value.Substring(1, value.Length - 2);
        }
        
        // Escapar comillas internas para PostgreSQL
        value = value.Replace("'", "''");
        
        // Devolver como string quoted
        return "'" + value + "'";
    }

    /// <summary>
    /// Limpia valores string simples
    /// </summary>
    private string CleanSimpleStringValue(string value)
    {
        value = value.Trim();
        
        if (string.Equals(value, "NULL", StringComparison.OrdinalIgnoreCase))
            return "NULL";
        
        // Remover comillas externas si existen
        if (value.StartsWith("'") && value.EndsWith("'"))
        {
            value = value.Substring(1, value.Length - 2);
        }
        
        // Escapar comillas simples
        value = value.Replace("'", "''");
        
        return "'" + value + "'";
    }

    /// <summary>
    /// Obtiene las dependencias de clave foránea entre tablas
    /// </summary>
    private async Task<Dictionary<string, List<string>>> GetTableDependenciesAsync()
    {
        var dependencies = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        
        try
        {
            var connection = _dbContext.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    tc.table_name AS dependent_table,
                    ccu.table_name AS referenced_table
                FROM information_schema.table_constraints tc
                JOIN information_schema.key_column_usage kcu 
                    ON tc.constraint_name = kcu.constraint_name
                    AND tc.table_schema = kcu.table_schema
                JOIN information_schema.constraint_column_usage ccu 
                    ON ccu.constraint_name = tc.constraint_name
                    AND ccu.table_schema = tc.table_schema
                WHERE tc.constraint_type = 'FOREIGN KEY'
                    AND tc.table_schema = 'public'
                ORDER BY tc.table_name, ccu.table_name";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var dependentTable = reader.GetString(0);
                var referencedTable = reader.GetString(1);
                
                // No auto-referencias
                if (!dependentTable.Equals(referencedTable, StringComparison.OrdinalIgnoreCase))
                {
                    if (!dependencies.ContainsKey(dependentTable))
                        dependencies[dependentTable] = new List<string>();
                    
                    if (!dependencies[dependentTable].Contains(referencedTable, StringComparer.OrdinalIgnoreCase))
                    {
                        dependencies[dependentTable].Add(referencedTable);
                    }
                }
            }
            
            _logger.LogDebug("Dependencias de tablas encontradas: {Dependencies}", 
                string.Join(", ", dependencies.Select(d => $"{d.Key} -> [{string.Join(", ", d.Value)}]")));
            
            return dependencies;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error obteniendo dependencias de tablas");
            return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Ordena las tablas en orden topológico para respetar dependencias de clave foránea
    /// </summary>
    private List<string> OrderTablesByDependencies(List<string> tables, Dictionary<string, List<string>> dependencies)
    {
        var orderedTables = new List<string>();
        var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var processing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var table in tables)
        {
            ProcessTableDependencies(table, tables, dependencies, orderedTables, processed, processing);
        }
        
        // Agregar cualquier tabla que no se haya procesado (sin dependencias)
        foreach (var table in tables)
        {
            if (!processed.Contains(table))
            {
                orderedTables.Add(table);
                processed.Add(table);
            }
        }
        
        _logger.LogInformation("Orden de procesamiento de tablas: {TableOrder}", string.Join(" -> ", orderedTables));
        
        return orderedTables;
    }

    /// <summary>
    /// Procesa recursivamente las dependencias de una tabla (algoritmo de ordenamiento topológico)
    /// </summary>
    private void ProcessTableDependencies(string table, List<string> allTables, 
        Dictionary<string, List<string>> dependencies, List<string> orderedTables, 
        HashSet<string> processed, HashSet<string> processing)
    {
        // Si ya fue procesada, retornar
        if (processed.Contains(table))
            return;
        
        // Detectar referencias circulares
        if (processing.Contains(table))
        {
            _logger.LogWarning("Referencia circular detectada en tabla: {Table}", table);
            return;
        }
        
        // Marcar como en proceso
        processing.Add(table);
        
        // Procesar dependencias primero
        if (dependencies.ContainsKey(table))
        {
            foreach (var dependency in dependencies[table])
            {
                // Solo procesar dependencias que están en la lista de tablas a exportar/importar
                if (allTables.Contains(dependency, StringComparer.OrdinalIgnoreCase))
                {
                    ProcessTableDependencies(dependency, allTables, dependencies, orderedTables, processed, processing);
                }
            }
        }
        
        // Marcar como procesada y agregar a la lista ordenada
        processing.Remove(table);
        if (!processed.Contains(table))
        {
            processed.Add(table);
            orderedTables.Add(table);
        }
    }

    /// <summary>
    /// Ordena las declaraciones SQL por dependencias de tabla
    /// </summary>
    private List<string> OrderStatementsByTableDependencies(List<string> statements, Dictionary<string, List<string>> dependencies)
    {
        try
        {
            // Agrupar declaraciones por tabla
            var statementsByTable = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var unidentifiedStatements = new List<string>();
            
            foreach (var statement in statements)
            {
                var tableName = ExtractTableNameFromInsert(statement);
                if (!string.IsNullOrEmpty(tableName))
                {
                    if (!statementsByTable.ContainsKey(tableName))
                        statementsByTable[tableName] = new List<string>();
                    
                    statementsByTable[tableName].Add(statement);
                }
                else
                {
                    unidentifiedStatements.Add(statement);
                }
            }
            
            // Ordenar tablas por dependencias
            var orderedTables = OrderTablesByDependencies(statementsByTable.Keys.ToList(), dependencies);
            
            // Reconstruir lista de declaraciones en el orden correcto
            var orderedStatements = new List<string>();
            
            // Agregar declaraciones en orden de dependencias
            foreach (var tableName in orderedTables)
            {
                if (statementsByTable.ContainsKey(tableName))
                {
                    orderedStatements.AddRange(statementsByTable[tableName]);
                }
            }
            
            // Agregar declaraciones no identificadas al final
            orderedStatements.AddRange(unidentifiedStatements);
            
            _logger.LogInformation("Declaraciones SQL ordenadas por dependencias. Orden de tablas: {TableOrder}", 
                string.Join(" -> ", orderedTables));
            
            return orderedStatements;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error ordenando declaraciones por dependencias, usando orden original");
            return statements;
        }
    }

    #endregion
}
