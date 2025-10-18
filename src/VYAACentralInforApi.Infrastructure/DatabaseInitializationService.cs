namespace VYAACentralInforApi.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VYAACentralInforApi.ApplicationCore.System.Interfaces;
using VYAACentralInforApi.Infrastructure.Sales.Data;
using VYAACentralInforApi.Infrastructure.Sales.Services;



public class DatabaseInitializationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializationService> _logger;
    private readonly SalesMongoDbContext _salesContext;

    public DatabaseInitializationService(IServiceProvider serviceProvider, ILogger<DatabaseInitializationService> logger, SalesMongoDbContext salesContext)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _salesContext = salesContext;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        
        try
        {
            _logger.LogInformation("Iniciando inicialización general de la base de datos...");
            
            // Inicializar datos del módulo System
            await InitializeSystemModuleAsync(scope.ServiceProvider);
            
            // Inicializar datos del módulo Sales
            await InitializeSalesModuleAsync(scope.ServiceProvider);
            
            // Ejecutar migración de cotizaciones
            //await MigrateQuotationsAsync();
            
            _logger.LogInformation("Inicialización general de la base de datos completada exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la inicialización general de la base de datos");
            throw; // Re-lanzar para que la aplicación no inicie con datos corruptos
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deteniendo servicio de inicialización general de base de datos...");
        return Task.CompletedTask;
    }

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
    /*
    private async Task MigrateQuotationsAsync()
    {
        try
        {
            _logger.LogInformation("Iniciando migración de cotizaciones...");
            //var migrationService = new QuotationMigrationService(_salesContext);
            //await migrationService.MigrateQuotationsAsync();
            _logger.LogInformation("Migración de cotizaciones completada.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la migración de cotizaciones");
            throw;
        }
    }
    */
}