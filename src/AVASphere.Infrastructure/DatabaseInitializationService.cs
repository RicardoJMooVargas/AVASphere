using AVASphere.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AVASphere.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AVASphere.ApplicationCore.System.Interfaces;
using Infrastructure.Sales.Data;
using Infrastructure.Sales.Services;



public class DatabaseInitializationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializationService> _logger;

    public DatabaseInitializationService(IServiceProvider serviceProvider, ILogger<DatabaseInitializationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    // Método para iniciar el servicio
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        try
        {
            _logger.LogInformation("Iniciando inicialización general de la base de datos...");

            // Ejecuta migraciones de PostgreSQL
            await InitializePostgreSqlAsync(scope.ServiceProvider);

            // Inicializar datos del módulo System (Mongo)
            await InitializeSystemModuleAsync(scope.ServiceProvider);

            // Inicializar datos del módulo Sales (Mongo)
            await InitializeSalesModuleAsync(scope.ServiceProvider);

            _logger.LogInformation("Inicialización completada exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la inicialización general de la base de datos");
            throw;
        }
    }
    // Método para detener el servicio (no se usa en este caso)
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deteniendo servicio de inicialización general de base de datos...");
        return Task.CompletedTask;
    }
    // Método específico para inicializar el módulo System
    private async Task InitializeSystemModuleAsync(IServiceProvider serviceProvider)
    {
        try
        {
            var userService = serviceProvider.GetRequiredService<IUserService>();
            
            _logger.LogInformation("Inicializando datos por defecto del módulo System...");
            await userService.InitializeDefaultUserAsync();
            _logger.LogInformation("Módulo System inicializado correctamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar el módulo System");
            throw;
        }
    }
    // Método para futuras inicializaciones de otros módulos
    private async Task InitializeSalesModuleAsync(IServiceProvider serviceProvider)
    {
        try
        {
            _logger.LogInformation("Inicializando datos por defecto del módulo Sales...");
            // var salesService = serviceProvider.GetRequiredService<ISalesService>();
            // await salesService.InitializeDefaultDataAsync();
            _logger.LogInformation("Módulo Sales inicializado correctamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar el módulo Sales");
            throw;
        }
    }
    // Método para aplicar migraciones pendientes en PostgreSQL
    private async Task InitializePostgreSqlAsync(IServiceProvider serviceProvider)
    {
        try
        {
            _logger.LogInformation("Iniciando configuración de PostgreSQL...");
            var dbContext = serviceProvider.GetRequiredService<CommonDbContext>();

            // Verificar que el contexto puede conectarse
            var canConnect = await dbContext.Database.CanConnectAsync();
            if (!canConnect)
            {
                _logger.LogError("No se puede conectar a la base de datos PostgreSQL");
                throw new InvalidOperationException("No se puede conectar a PostgreSQL");
            }

            _logger.LogInformation("Conexión a PostgreSQL verificada exitosamente.");

            // Verificar si la tabla de historial de migraciones existe
            var historyTableExists = await CheckMigrationHistoryTableExistsAsync(dbContext);
            
            if (!historyTableExists)
            {
                _logger.LogInformation("Tabla de historial de migraciones no existe. Verificando si las tablas ya existen...");
                
                var tableExists = await CheckConfigSysTableExistsAsync(dbContext);
                
                if (tableExists)
                {
                    _logger.LogInformation("Las tablas ya existen. Creando historial de migraciones...");

                    await dbContext.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                            ""MigrationId"" character varying(150) NOT NULL,
                            ""ProductVersion"" character varying(32) NOT NULL,
                            CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
                        );
                    ");

                    await dbContext.Database.ExecuteSqlRawAsync(@"
                        INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                        VALUES ('20251023101356_InitialCreate', '9.0.0')
                        ON CONFLICT DO NOTHING;
                    ");

                    _logger.LogInformation("Historial de migraciones sincronizado.");
                }
                else
                {
                    _logger.LogInformation("Las tablas no existen. Aplicando migraciones normalmente...");
                    await dbContext.Database.MigrateAsync();
                }
            }
            else
            {
                // El historial existe, proceder normalmente con migraciones pendientes
                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
                
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation($"Aplicando {pendingMigrations.Count()} migraciones pendientes...");
                    foreach (var migration in pendingMigrations)
                    {
                        _logger.LogInformation($"- {migration}");
                    }
                    
                    await dbContext.Database.MigrateAsync();
                    _logger.LogInformation("Migraciones aplicadas correctamente.");
                }
                else
                {
                    _logger.LogInformation("No hay migraciones pendientes. Base de datos PostgreSQL actualizada.");
                }
            }

            _logger.LogInformation("Configuración de PostgreSQL completada exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la inicialización/migración de PostgreSQL");
            throw;
        }
    }

    private async Task<bool> CheckMigrationHistoryTableExistsAsync(CommonDbContext dbContext)
    {
        try
        {
            var query = @"
                SELECT EXISTS (
                    SELECT 1 FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                    AND table_name = '__EFMigrationsHistory'
                )";
            
            var connection = dbContext.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = query;
            var result = await command.ExecuteScalarAsync();
            
            return Convert.ToBoolean(result);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error verificando tabla de historial de migraciones");
            return false;
        }
    }

    private async Task<bool> CheckConfigSysTableExistsAsync(CommonDbContext dbContext)
    {
        try
        {
            var query = @"
                SELECT EXISTS (
                    SELECT 1 FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                    AND table_name = 'config_sys'
                )";
            
            var connection = dbContext.Database.GetDbConnection();
            if (connection.State != global::System.Data.ConnectionState.Open)
                await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = query;
            var result = await command.ExecuteScalarAsync();
            
            return Convert.ToBoolean(result);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error verificando tabla config_sys");
            return false;
        }
    }

    private async Task CreateMigrationHistoryAndMarkAsAppliedAsync(CommonDbContext dbContext)
    {
        try
        {
            // Crear la tabla de historial de migraciones
            var createHistoryTableSql = @"
                CREATE TABLE ""__EFMigrationsHistory"" (
                    ""MigrationId"" character varying(150) NOT NULL,
                    ""ProductVersion"" character varying(32) NOT NULL,
                    CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
                );";
            
            await dbContext.Database.ExecuteSqlRawAsync(createHistoryTableSql);
            
            // Obtener todas las migraciones disponibles
            var allMigrations = dbContext.Database.GetMigrations();
            
            // Marcar todas las migraciones como aplicadas
            foreach (var migration in allMigrations)
            {
                var insertSql = @"
                    INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"") 
                    VALUES ({0}, {1})
                    ON CONFLICT (""MigrationId"") DO NOTHING;";
                
                await dbContext.Database.ExecuteSqlRawAsync(insertSql, migration, "9.0.0");
                _logger.LogInformation($"Migración marcada como aplicada: {migration}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el historial de migraciones");
            throw;
        }
    }
}