using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AVASphere.ApplicationCore.System.Interfaces;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Projects.Interfaces;
using AVASphere.Infrastructure.System.Services;
using AVASphere.Infrastructure.System.Configuration;
using AVASphere.Infrastructure.Common.Data.Repositories;
using AVASphere.Infrastructure.Common.Repository;
using AVASphere.Infrastructure.Common.Security;
using AVASphere.Infrastructure.Common.Services;
using AVASphere.Infrastructure.Projects.Repository;
using AVASphere.Infrastructure.Projects.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql; // ✅ AGREGAR ESTE USING

// Sales module usings
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.Infrastructure.Sales.Repositories;
using AVASphere.Infrastructure.Sales.Services;

// Inventory module usings
using AVASphere.ApplicationCore.Inventory.Interfaces;
using AVASphere.Infrastructure.Inventory.Services;
using AVASphere.Infrastructure.Inventory.Repository;

namespace AVASphere.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // CONFIGURACIONES DE BASES DE DATOS 
        AddPostgreSqlConfiguration(services, configuration);

        // Configuración de JWT Authentication
        AddJwtAuthentication(services, configuration);

        // Registrar servicios por módulo
        AddInitializationServices(services);
        AddSystemServices(services);
        AddSalesServices(services);
        AddInventoryServices(services);
        AddCommonServices(services);

        return services;
    }

    private static void AddPostgreSqlConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
                               ?? configuration.GetSection("DbSettings:ConnectionString").Value
                               ?? "Host=localhost;Port=5432;Database=AVASphereDB;Username=postgres;Password=postgres;";

        // Configurar Npgsql para JSON dinámico
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        // Configurar el data source con JSON dinámico habilitado
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<MasterDbContext>(options =>
            options.UseNpgsql(dataSource));
    }

    private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings
        {
            Key = Environment.GetEnvironmentVariable("JWT_KEY")
                  ?? configuration["JwtSettings:Key"]
                  ?? "VYAACentralInforApiSecretKey2024!@#$%^&*()",
            Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                     ?? configuration["JwtSettings:Issuer"]
                     ?? "VYAACentralInforApi",
            Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                       ?? configuration["JwtSettings:Audience"]
                       ?? "VYAACentralInforApiUsers",
            ExpirationInMinutes = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES"), out var exp)
                                  ? exp
                                  : (configuration.GetValue<int?>("JwtSettings:ExpirationInMinutes") ?? 60)
        };

        services.AddSingleton(jwtSettings);
        services.AddTransient<ITokenService, TokenService>();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Key)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
    }

    private static void AddInitializationServices(IServiceCollection services)
    {
        services.AddHostedService<DatabaseInitializationService>();
    }

    private static void AddSystemServices(IServiceCollection services)
    {
        services.AddScoped<DbToolsServices>();
        services.AddScoped<DatabaseMigrationService>();
        services.AddScoped<BackupService>(); // Agregado para ConfigController
    }

    private static void AddSalesServices(IServiceCollection services)
    {
        // Registrar repositorios y servicios del módulo Sales
        services.AddHttpClient();
        services.AddScoped<IQuotationRepository, QuotationRepository>();
        services.AddScoped<IQuotationService, QuotationService>();

        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<ISaleService, SaleService>();

        services.AddScoped<ISaleQuotationRepository, SaleQuotationRepository>();
        services.AddScoped<ISaleQuotationService, SaleQuotationService>();

        services.AddScoped<IQuotationVersionRepository, QuotationVersionRepository>();
        services.AddScoped<IQuotationVersionService, QuotationVersionService>();

        services.AddScoped<IExternalSalesRepository, ExternalSalesRepository>();
        services.AddScoped<IExternalSalesService, ExternalSalesService>();

        services.AddScoped<ISaleChartService, SaleChartService>();
    }

    private static void AddCommonServices(IServiceCollection services)
    {
        services.AddScoped<IConfigSysRepository, ConfigSysRepository>();
        services.AddScoped<IConfigSysService, ConfigSysService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAreaRepository, AreaRepository>();
        services.AddScoped<IAreaService, AreaService>();
        services.AddScoped<IRolRepository, RolRepository>();
        services.AddScoped<IRolService, RolService>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<IPropertyValueRepository, PropertyValueRepository>();
        services.AddScoped<IPropertyValueService, PropertyValueService>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<ISupplierService, SupplierService>();

        // Servicios de seguridad
        services.AddScoped<IEncryptionService, EncryptionService>();

        // Project Category
        services.AddScoped<IProjectCategoryService, ProjectCategoryService>();
        services.AddScoped<IProjectCategoryRepository, ProjectCategoryRepository>();

        // Project
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ListOfCategoriesRepository>();

        // Product Services
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();

    }

    private static void AddInventoryServices(IServiceCollection services)
    {
        // Inventory Service
        services.AddScoped<IInventoryService, InventoryService>();

        // Inventory Repository
        services.AddScoped<IInventoryRepository, InventoryRepository>();

        // Warehouse
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();

        // Physical Inventory
        services.AddScoped<IPhysicalInventoryRepository, PhysicalInventoryRepository>();
        services.AddScoped<IPhysicalInventoryDetailRepository, PhysicalInventoryDetailRepository>();
        services.AddScoped<IPhysicalInventoryService, PhysicalInventoryService>();
        services.AddScoped<IPhysicalInventoryDetailService, PhysicalInventoryDetailService>();

        // Storage Structure and Location Details
        services.AddScoped<IStorageStructureRepository, StorageStructureRepository>();
        services.AddScoped<IStorageStructureService, StorageStructureService>();
        services.AddScoped<ILocationDetailsRepository, LocationDetailsRepository>();
        services.AddScoped<ILocationDetailsService, LocationDetailsService>();

        // Stock Movement
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();

        // Warehouse Transfer
        services.AddScoped<IWarehouseTransferRepository, WarehouseTransferRepository>();
        services.AddScoped<IWarehouseTransferDetailRepository, WarehouseTransferDetailRepository>();
    }
}