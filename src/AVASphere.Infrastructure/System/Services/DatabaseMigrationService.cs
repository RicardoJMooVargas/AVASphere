﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AVASphere.Infrastructure.System.Services
{
    public class DatabaseMigrationService
    {
        private readonly ILogger<DatabaseMigrationService> _logger;
        private readonly MasterDbContext _dbContext;

        public DatabaseMigrationService(ILogger<DatabaseMigrationService> logger, MasterDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<string> ApplyMigrationsAsync()
        {
            MasterDbContext contextToUse = null;
            bool usingFactory = false;
            
            try
            {
                _logger.LogInformation("Intentando conectar usando IDesignTimeDbContextFactory...");

                // Intentar primero con el factory
                var factory = new MasterDbContextFactory();
                var factoryContext = factory.CreateDbContext(Array.Empty<string>());
                
                var canConnectFactory = await factoryContext.Database.CanConnectAsync();
                if (canConnectFactory)
                {
                    _logger.LogInformation("✅ Conexión exitosa con IDesignTimeDbContextFactory");
                    contextToUse = factoryContext;
                    usingFactory = true;
                }
                else
                {
                    factoryContext.Dispose();
                    _logger.LogWarning("❌ Factory falló, intentando con contexto del DI...");
                    
                    var canConnectDI = await _dbContext.Database.CanConnectAsync();
                    if (canConnectDI)
                    {
                        _logger.LogInformation("✅ Conexión exitosa con contexto del DI");
                        contextToUse = _dbContext;
                        usingFactory = false;
                    }
                    else
                    {
                        return "❌ No se puede conectar a la base de datos con ningún método. Verificar configuración.";
                    }
                }

                _logger.LogInformation($"Usando contexto: {(usingFactory ? "IDesignTimeDbContextFactory" : "DI Container")}");
                
                // Log de la cadena de conexión que se está usando
                var currentConnectionString = contextToUse.Database.GetConnectionString();
                _logger.LogInformation($"📍 Cadena de conexión: {currentConnectionString}");
                _logger.LogInformation($"🏗️ Base de datos destino: {ExtractDatabaseFromConnectionString(currentConnectionString)}");

                // Obtener información de migraciones
                var allMigrations = contextToUse.Database.GetMigrations();
                var appliedMigrations = await contextToUse.Database.GetAppliedMigrationsAsync();
                var pendingMigrations = await contextToUse.Database.GetPendingMigrationsAsync();

                var allMigrationsList = allMigrations.ToList();
                var appliedMigrationsList = appliedMigrations.ToList();
                var pendingMigrationsList = pendingMigrations.ToList();

                _logger.LogInformation($"Migraciones en assembly: {allMigrationsList.Count}");
                _logger.LogInformation($"Migraciones aplicadas: {appliedMigrationsList.Count}");
                _logger.LogInformation($"Migraciones pendientes: {pendingMigrationsList.Count}");

                if (pendingMigrationsList.Count == 0)
                {
                    if (allMigrationsList.Count == 0)
                    {
                        return "⚠️ No se encontraron migraciones en el assembly. Verificar que existan archivos en System/Migrations.";
                    }
                    return $"✅ Base de datos actualizada. {appliedMigrationsList.Count} de {allMigrationsList.Count} migraciones aplicadas.";
                }

                // Aplicar migraciones pendientes
                _logger.LogInformation($"Aplicando {pendingMigrationsList.Count} migraciones pendientes...");
                
                // Siempre crear un contexto especial que ignore PendingModelChangesWarning
                // Esto es necesario para inicialización de DB en nuevos entornos
                var connectionString = contextToUse.Database.GetConnectionString();
                var optionsBuilder = new DbContextOptionsBuilder<MasterDbContext>();
                optionsBuilder.UseNpgsql(connectionString)
                             .ConfigureWarnings(warnings => 
                                 warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

                using var migrationContext = new MasterDbContext(optionsBuilder.Options);
                _logger.LogInformation("Aplicando migraciones con advertencias suprimidas para inicialización de DB...");
                await migrationContext.Database.MigrateAsync();

                // Verificar resultado final
                var finalAppliedMigrations = await contextToUse.Database.GetAppliedMigrationsAsync();
                var finalCount = finalAppliedMigrations.Count();

                return $"✅ Migraciones aplicadas exitosamente usando {(usingFactory ? "Factory" : "DI")}. Total: {finalCount} migraciones.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aplicando migraciones");
                return $"❌ Error aplicando migraciones: {ex.Message}";
            }
            finally
            {
                // Solo liberar el contexto si usamos factory
                if (usingFactory && contextToUse != null)
                {
                    contextToUse.Dispose();
                }
            }
        }

        /// <summary>
        /// Diagnóstico simple del estado de las migraciones
        /// </summary>
        public async Task<string> DiagnoseMigrationsAsync()
        {
            MasterDbContext contextToUse = null;
            bool usingFactory = false;
            
            try
            {
                _logger.LogInformation("Iniciando diagnóstico de migraciones...");

                // Intentar primero con factory, luego con DI
                var factory = new MasterDbContextFactory();
                var factoryContext = factory.CreateDbContext(Array.Empty<string>());
                
                var canConnectFactory = await factoryContext.Database.CanConnectAsync();
                if (canConnectFactory)
                {
                    contextToUse = factoryContext;
                    usingFactory = true;
                }
                else
                {
                    factoryContext.Dispose();
                    var canConnectDI = await _dbContext.Database.CanConnectAsync();
                    if (canConnectDI)
                    {
                        contextToUse = _dbContext;
                        usingFactory = false;
                    }
                    else
                    {
                        return "❌ No se puede conectar a la base de datos con ningún método.";
                    }
                }

                var connectionString = contextToUse.Database.GetConnectionString();
                var allMigrations = contextToUse.Database.GetMigrations().ToList();
                var appliedMigrations = (await contextToUse.Database.GetAppliedMigrationsAsync()).ToList();
                var pendingMigrations = (await contextToUse.Database.GetPendingMigrationsAsync()).ToList();

                var diagnosis = "=== DIAGNÓSTICO DE MIGRACIONES ===\n";
                diagnosis += $"🔌 Conexión: ✅ OK ({(usingFactory ? "Factory" : "DI")})\n";
                diagnosis += $"📍 Connection String: {connectionString}\n";
                diagnosis += $"📦 Migraciones en Assembly: {allMigrations.Count}\n";
                
                if (allMigrations.Any())
                {
                    foreach (var migration in allMigrations)
                    {
                        diagnosis += $"  ✅ {migration}\n";
                    }
                }
                else
                {
                    diagnosis += "  ⚠️ No se encontraron migraciones en el assembly\n";
                    diagnosis += "  💡 Verificar archivos en System/Migrations\n";
                }

                diagnosis += $"\n🗃️ Migraciones Aplicadas: {appliedMigrations.Count}\n";
                foreach (var migration in appliedMigrations)
                {
                    diagnosis += $"  ✅ {migration}\n";
                }

                diagnosis += $"\n⏳ Migraciones Pendientes: {pendingMigrations.Count}\n";
                foreach (var migration in pendingMigrations)
                {
                    diagnosis += $"  🔄 {migration}\n";
                }

                return diagnosis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en diagnóstico de migraciones");
                return $"❌ Error en diagnóstico: {ex.Message}";
            }
            finally
            {
                if (usingFactory && contextToUse != null)
                {
                    contextToUse.Dispose();
                }
            }
        }

        /// <summary>
        /// Aplica migraciones suprimiendo PendingModelChangesWarning - Específico para inicialización de DB
        /// </summary>
        public async Task<string> ApplyMigrationsForceAsync()
        {
            try
            {
                _logger.LogInformation("Aplicando migraciones con supresión forzada de PendingModelChangesWarning...");

                // Usar factory primero, luego DI como respaldo
                string connectionString;
                try
                {
                    var factory = new MasterDbContextFactory();
                    using var testContext = factory.CreateDbContext(Array.Empty<string>());
                    var canConnect = await testContext.Database.CanConnectAsync();
                    if (canConnect)
                    {
                        connectionString = testContext.Database.GetConnectionString();
                        _logger.LogInformation("Usando configuración del Factory para migración forzada");
                    }
                    else
                    {
                        throw new InvalidOperationException("Factory no puede conectar");
                    }
                }
                catch
                {
                    connectionString = _dbContext.Database.GetConnectionString();
                    _logger.LogInformation("Usando configuración del DI para migración forzada");
                }

                // Crear contexto que ignore completamente PendingModelChangesWarning
                var optionsBuilder = new DbContextOptionsBuilder<MasterDbContext>();
                optionsBuilder.UseNpgsql(connectionString)
                             .ConfigureWarnings(warnings => 
                                 warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

                using var migrationContext = new MasterDbContext(optionsBuilder.Options);

                // Verificar conexión
                var canConnectMigration = await migrationContext.Database.CanConnectAsync();
                if (!canConnectMigration)
                {
                    return "❌ No se puede conectar con el contexto de migración.";
                }

                // Obtener información de migraciones
                var allMigrations = migrationContext.Database.GetMigrations().ToList();
                var appliedMigrations = (await migrationContext.Database.GetAppliedMigrationsAsync()).ToList();
                var pendingMigrations = (await migrationContext.Database.GetPendingMigrationsAsync()).ToList();

                _logger.LogInformation($"Migraciones en assembly: {allMigrations.Count}");
                _logger.LogInformation($"Migraciones aplicadas: {appliedMigrations.Count}");
                _logger.LogInformation($"Migraciones pendientes: {pendingMigrations.Count}");

                if (pendingMigrations.Count == 0)
                {
                    return $"✅ Base de datos actualizada. {appliedMigrations.Count} de {allMigrations.Count} migraciones aplicadas.";
                }

                // Aplicar migraciones con advertencias suprimidas
                _logger.LogInformation($"Aplicando {pendingMigrations.Count} migraciones con advertencias suprimidas...");
                foreach (var migration in pendingMigrations)
                {
                    _logger.LogInformation($"Aplicando: {migration}");
                }

                await migrationContext.Database.MigrateAsync();

                // Verificar resultado final
                var finalAppliedMigrations = (await migrationContext.Database.GetAppliedMigrationsAsync()).ToList();
                _logger.LogInformation($"✅ Migraciones aplicadas exitosamente. Total final: {finalAppliedMigrations.Count}");

                return $"✅ Migraciones aplicadas exitosamente (forzado). Total: {finalAppliedMigrations.Count} migraciones en la base de datos.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aplicando migraciones forzadas");
                return $"❌ Error aplicando migraciones forzadas: {ex.Message}";
            }
        }

        /// <summary>
        /// Marca las migraciones existentes como aplicadas en __EFMigrationsHistory cuando las tablas ya existen
        /// </summary>
        public async Task<string> SynchronizeMigrationHistoryAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando sincronización de historial de migraciones...");
                
                var factory = new MasterDbContextFactory();
                using var context = factory.CreateDbContext(Array.Empty<string>());
                
                // Verificar que podamos conectar
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return "❌ No se puede conectar a la base de datos";
                }

                // Obtener todas las migraciones del assembly
                var migrations = context.Database.GetMigrations().ToList();
                _logger.LogInformation($"📋 Migraciones encontradas en el código: {migrations.Count}");
                
                // Obtener migraciones ya aplicadas
                var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                _logger.LogInformation($"📋 Migraciones aplicadas en BD: {appliedMigrations.Count()}");

                // Encontrar migraciones faltantes en el historial
                var missingMigrations = migrations.Except(appliedMigrations).ToList();
                
                if (missingMigrations.Count == 0)
                {
                    return "✅ Todas las migraciones ya están sincronizadas";
                }

                _logger.LogInformation($"📝 Migraciones faltantes en historial: {missingMigrations.Count}");
                
                // Insertar manualmente las migraciones faltantes en __EFMigrationsHistory
                using var connection = context.Database.GetDbConnection();
                await connection.OpenAsync();
                
                foreach (var migrationId in missingMigrations)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"") 
                        VALUES (@migrationId, @productVersion)
                        ON CONFLICT (""MigrationId"") DO NOTHING";
                    
                    var migrationParam = command.CreateParameter();
                    migrationParam.ParameterName = "@migrationId";
                    migrationParam.Value = migrationId;
                    command.Parameters.Add(migrationParam);
                    
                    var versionParam = command.CreateParameter();
                    versionParam.ParameterName = "@productVersion";
                    versionParam.Value = "8.0.0"; // Versión de EF Core
                    command.Parameters.Add(versionParam);
                    
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInformation($"✅ Migración {migrationId} marcada como aplicada");
                }

                return $"✅ {missingMigrations.Count} migraciones sincronizadas correctamente en __EFMigrationsHistory";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sincronizando historial de migraciones");
                return $"❌ Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Verifica qué tablas existen realmente en la base de datos
        /// </summary>
        public async Task<string> CheckDatabaseTablesAsync()
        {
            try
            {
                _logger.LogInformation("Verificando tablas existentes en la base de datos...");
                
                var factory = new MasterDbContextFactory();
                using var context = factory.CreateDbContext(Array.Empty<string>());
                
                // Verificar que podamos conectar
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return "❌ No se puede conectar a la base de datos";
                }

                var connectionString = context.Database.GetConnectionString();
                _logger.LogInformation($"🏗️ Verificando tablas en: {ExtractDatabaseFromConnectionString(connectionString)}");

                using var connection = context.Database.GetDbConnection();
                await connection.OpenAsync();
                
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT tablename 
                    FROM pg_tables 
                    WHERE schemaname = 'public'
                    ORDER BY tablename";
                
                var tables = new List<string>();
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }

                var result = $"📊 TABLAS EN BASE DE DATOS: {ExtractDatabaseFromConnectionString(connectionString)}\n";
                result += $"🔢 Total de tablas: {tables.Count}\n\n";
                
                if (tables.Any())
                {
                    foreach (var table in tables)
                    {
                        result += $"  📋 {table}\n";
                    }
                }
                else
                {
                    result += "  ⚠️ No hay tablas en la base de datos\n";
                    result += "  💡 La base de datos está vacía\n";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando tablas de la base de datos");
                return $"❌ Error: {ex.Message}";
            }
        }

        private static string ExtractDatabaseFromConnectionString(string? connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return "UNKNOWN";
                
            var parts = connectionString.Split(';');
            var dbPart = parts.FirstOrDefault(p => p.Trim().StartsWith("Database=", StringComparison.OrdinalIgnoreCase));
            
            if (dbPart != null)
            {
                return dbPart.Split('=')[1].Trim();
            }
            
            return "NOT_FOUND";
        }
    }
}

