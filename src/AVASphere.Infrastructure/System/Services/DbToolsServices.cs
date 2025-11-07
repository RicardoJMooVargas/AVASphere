using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AVASphere.Infrastructure.System.Services
{
    public class DbToolsServices
    {
        private readonly MasterDbContext _dbContext;
        private readonly ILogger<DbToolsServices> _logger;
        private readonly IConfiguration _configuration;

        public DbToolsServices(MasterDbContext dbContext, ILogger<DbToolsServices> logger, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _logger = logger;
            _configuration = configuration;
        }

        // 1️⃣ Verificar conexión y existencia de datos
        public async Task<(bool IsConnected, bool HasData, string Message)> CheckConnectionAsync()
        {
            try
            {
                // 1️⃣ Verificar conexión
                var canConnect = await _dbContext.Database.CanConnectAsync();
                if (!canConnect)
                    return (false, false, "No se pudo conectar a la base de datos.");

                // 2️⃣ Revisar si existen tablas (excluyendo __EFMigrationsHistory)
                int tableCount = 0;
                bool configSysTableExists = false;
                int configSysRecordsCount = 0;

                await using (var connection = _dbContext.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    
                    // Verificar si existen tablas de datos
                    await using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                    SELECT COUNT(*) 
                    FROM information_schema.tables 
                    WHERE table_schema = 'public'
                    AND table_name != '__EFMigrationsHistory';";

                        var result = await command.ExecuteScalarAsync();
                        tableCount = Convert.ToInt32(result);
                    }

                    // 3️⃣ Verificar específicamente si existe la tabla ConfigSys
                    if (tableCount > 0)
                    {
                        await using (var configSysTableCommand = connection.CreateCommand())
                        {
                            configSysTableCommand.CommandText = @"
                        SELECT COUNT(*) 
                        FROM information_schema.tables 
                        WHERE table_schema = 'public' 
                        AND table_name = 'ConfigSys';";

                            var configSysTableResult = await configSysTableCommand.ExecuteScalarAsync();
                            configSysTableExists = Convert.ToInt32(configSysTableResult) > 0;
                        }

                        // 4️⃣ Si existe la tabla ConfigSys, verificar si tiene registros
                        if (configSysTableExists)
                        {
                            try
                            {
                                await using (var configSysRecordsCommand = connection.CreateCommand())
                                {
                                    configSysRecordsCommand.CommandText = @"SELECT COUNT(*) FROM ""ConfigSys"";";
                                    var configSysRecordsResult = await configSysRecordsCommand.ExecuteScalarAsync();
                                    configSysRecordsCount = Convert.ToInt32(configSysRecordsResult);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Error verificando registros en ConfigSys: {ex.Message}");
                            }
                        }
                    }
                }

                // 5️⃣ Determinar si hay datos significativos
                var hasData = tableCount > 0;
                var hasValidDatabase = configSysTableExists && configSysRecordsCount > 0;

                // 6️⃣ Generar mensaje descriptivo
                string message;
                if (tableCount == 0)
                {
                    message = "Conexión OK. No existen tablas de datos (base vacía o solo con historial de migraciones).";
                }
                else if (!configSysTableExists)
                {
                    message = $"Conexión OK. Existen {tableCount} tablas pero NO existe la tabla ConfigSys (formato incorrecto).";
                }
                else if (configSysRecordsCount == 0)
                {
                    message = $"Conexión OK. Existen {tableCount} tablas y tabla ConfigSys existe pero está vacía (sin configuración inicial).";
                }
                else
                {
                    message = $"Conexión OK. Base de datos válida con {tableCount} tablas y {configSysRecordsCount} registro(s) en ConfigSys.";
                }

                _logger.LogInformation($"Estado DB - Tablas: {tableCount}, ConfigSys existe: {configSysTableExists}, Registros ConfigSys: {configSysRecordsCount}");

                return (true, hasData, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comprobando conexión con la base de datos");
                return (false, false, $"Error: {ex.Message}");
            }
        }

        // 2️⃣ Crear una migración usando EF CLI directamente
        public async Task<string> CreateMigrationAsync(string migrationName)
        {
            try
            {
                // Construir rutas absolutas correctas
                var basePath = @"C:\Users\AcerLapTablet\repos\AVASphere";
                var infraProject = Path.Combine(basePath, "src", "AVASphere.Infrastructure", "AVASphere.Infrastructure.csproj");
                
                var command = $"dotnet ef migrations add {migrationName} " +
                             $"--project \"{infraProject}\" " +
                             $"--startup-project \"{infraProject}\" " +
                             $"--context MasterDbContext " +
                             $"--output-dir System/Migrations";

                _logger.LogInformation($"Ejecutando comando EF: {command}");
                var result = await ExecuteEfCommandAsync(command);

                if (result.Contains("Build failed"))
                {
                    return $"❌ Error de compilación. Ejecuta 'dotnet build' para ver los errores detallados.";
                }

                if (result.Contains("ERROR:") || result.Contains("error"))
                {
                    return $"❌ Error en EF: {result}";
                }

                return result.Contains("Done") || string.IsNullOrWhiteSpace(result) ? 
                    "✅ Migración creada exitosamente." : 
                    $"✅ {result}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando migración");
                return $"❌ Error creando migración: {ex.Message}";
            }
        }

        // 3️⃣ Aplicar migraciones usando EF CLI (igual que manual)
        public async Task<string> ApplyMigrationAsync()
        {
            try
            {
                // Construir rutas absolutas correctas
                var basePath = @"C:\Users\AcerLapTablet\repos\AVASphere";
                var infraProject = Path.Combine(basePath, "src", "AVASphere.Infrastructure", "AVASphere.Infrastructure.csproj");
                
                var command = $"dotnet ef database update " +
                             $"--project \"{infraProject}\" " +
                             $"--startup-project \"{infraProject}\" " +
                             $"--context MasterDbContext";

                _logger.LogInformation($"Ejecutando comando EF CLI: {command}");
                var result = await ExecuteEfCommandAsync(command);

                if (result.Contains("ERROR:") || result.Contains("error"))
                {
                    return $"❌ Error en EF CLI: {result}";
                }

                return result.Contains("Done") || string.IsNullOrWhiteSpace(result) ? 
                    "✅ Migraciones aplicadas correctamente." : 
                    $"✅ {result}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aplicando migraciones");
                return $"❌ Error aplicando migraciones: {ex.Message}";
            }
        }

        // 4️⃣ Eliminar la base de datos
        public async Task<string> DropDatabaseAsync()
        {
            try
            {
                var deleted = await _dbContext.Database.EnsureDeletedAsync();
                return deleted ? "🗑️ Base de datos eliminada correctamente." : "⚠️ No se eliminó ninguna base.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando la base de datos");
                return $"❌ Error eliminando base de datos: {ex.Message}";
            }
        }

        // 3️⃣ Eliminar solo las tablas (VERSIÓN DETALLADA CON CONFIGURACIÓN)
        public async Task<string> DropTablesAsync()
        {
            try
            {
                // Obtener connectionString desde IConfiguration
                var connectionString = _configuration.GetConnectionString("DefaultConnection")
                                      ?? _configuration.GetSection("DbSettings:ConnectionString").Value
                                      ?? "Host=localhost;Port=5432;Database=AVASphereDB;Username=postgres;Password=postgres;";

                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                // 1. Obtener lista de tablas
                await using var getTablesCmd = new NpgsqlCommand(@"
                    SELECT tablename 
                    FROM pg_tables 
                    WHERE schemaname = 'public' 
                    AND tablename != '__EFMigrationsHistory'", connection);
                
                var tables = new List<string>();
                await using (var reader = await getTablesCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        tables.Add(reader.GetString(0));
                    }
                }

                if (!tables.Any())
                {
                    return "ℹ️ No hay tablas que eliminar";
                }

                // 2. Deshabilitar constraints
                await using var disableCmd = new NpgsqlCommand("SET session_replication_role = 'replica'", connection);
                await disableCmd.ExecuteNonQueryAsync();

                // 3. Eliminar cada tabla
                foreach (var table in tables)
                {
                    try
                    {
                        await using var dropCmd = new NpgsqlCommand($"DROP TABLE IF EXISTS \"{table}\" CASCADE", connection);
                        await dropCmd.ExecuteNonQueryAsync();
                        _logger.LogInformation($"Tabla eliminada: {table}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error eliminando tabla {table}: {ex.Message}");
                    }
                }

                // 4. Rehabilitar constraints
                await using var enableCmd = new NpgsqlCommand("SET session_replication_role = 'origin'", connection);
                await enableCmd.ExecuteNonQueryAsync();

                return $"🗑️ {tables.Count} tablas eliminadas exitosamente";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando tablas");
                return $"❌ Error eliminando tablas: {ex.Message}";
            }
        }

        // Ejecutar comando EF CLI desde backend
        private static async Task<string> ExecuteEfCommandAsync(string command)
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            using var process = new Process();

            if (isWindows)
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c {command}";
            }
            else
            {
                process.StartInfo.FileName = "/bin/bash";
                process.StartInfo.Arguments = $"-c \"{command}\"";
            }

            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            // Establecer working directory desde la raíz del proyecto
            process.StartInfo.WorkingDirectory = @"C:\Users\AcerLapTablet\repos\AVASphere";

            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            var result = string.IsNullOrWhiteSpace(error) ? output : $"ERROR: {error}";
            
            // Si no hay output pero tampoco error, considerar éxito
            if (string.IsNullOrWhiteSpace(result) && process.ExitCode == 0)
            {
                return "Comando ejecutado exitosamente";
            }

            return result;
        }

        // 5️⃣ Proceso simplificado: eliminar tablas, crear y aplicar migración
        public async Task<string> RecreateDatabaseAsync(string migrationName = "Initial")
        {
            try
            {
                _logger.LogInformation("🔄 Iniciando recreación de base de datos...");
                var results = new List<string>();

                // 1. Eliminar todas las tablas
                _logger.LogInformation("🗑️ Eliminando tablas...");
                var dropResult = await DropTablesAsync();
                results.Add(dropResult);

                // 2. Crear migración
                _logger.LogInformation($"📝 Creando migración: {migrationName}...");
                var createResult = await CreateMigrationAsync(migrationName);
                results.Add(createResult);

                if (createResult.Contains("❌"))
                {
                    return string.Join("\n", results);
                }

                // 3. Aplicar migración
                _logger.LogInformation("⚙️ Aplicando migración...");
                var applyResult = await ApplyMigrationAsync();
                results.Add(applyResult);

                return string.Join("\n", results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en recreación de base de datos");
                return $"❌ Error: {ex.Message}";
            }
        }
    }
}