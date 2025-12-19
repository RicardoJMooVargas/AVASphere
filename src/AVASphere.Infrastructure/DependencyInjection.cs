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
using VYAACentralInforApi.ApplicationCore.Sales.Services;

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
    }

    private static void AddSalesServices(IServiceCollection services)
    {
        /// Registrar repositorios y servicios del módulo Sales
        services.AddHttpClient();
        services.AddScoped<IQuotationRepository, QuotationRepository>();
        services.AddScoped<IQuotationService, QuotationService>();

        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<ISaleService, SaleService>();

        services.AddScoped<ISaleQuotationRepository, SaleQuotationRepository>();
        services.AddScoped<ISaleQuotationService, SaleQuotationService>();

        services.AddScoped<IQuotationVersionRepository, QuotationVersionRepository>();
        services.AddScoped<IQuotationVersionService, QuotationVersionService>();
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
        

        // Servicios de seguridad
        services.AddScoped<IEncryptionService, EncryptionService>();
        
        // Project Category
        services.AddScoped<IProjectCategoryService, ProjectCategoryService>();
        services.AddScoped<IProjectCategoryRepository, ProjectCategoryRepository>();
        
    }
}