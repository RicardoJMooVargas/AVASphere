using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VYAACentralInforApi.Infrastructure.System.Configuration;
using VYAACentralInforApi.Infrastructure.System.Services;
using VYAACentralInforApi.Infrastructure.System.Data;
using VYAACentralInforApi.Infrastructure.System.Repositories;
using VYAACentralInforApi.Infrastructure.Sales.Data;
using VYAACentralInforApi.Infrastructure.Sales.Repositories;
using VYAACentralInforApi.ApplicationCore.Sales.Services;
using VYAACentralInforApi.ApplicationCore.System.Services;
using VYAACentralInforApi.ApplicationCore.System.Interfaces;
using VYAACentralInforApi.ApplicationCore.Sales.Interfaces;

namespace VYAACentralInforApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuración de MongoDB
        AddMongoDbConfiguration(services, configuration);
        
        // Configuración de JWT Authentication
        AddJwtAuthentication(services, configuration);
        
        // Registrar servicios del módulo System
        AddSystemServices(services);
        
        // Registrar servicios generales de inicialización
        AddInitializationServices(services);

        // Registrar servicios del módulo Sales
        AddSalesServices(services);

        return services;
    }

    private static void AddMongoDbConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        // Usar variables de entorno del archivo .env con fallback a configuración tradicional
        var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") 
                              ?? configuration.GetSection("MongoDB:ConnectionString").Value 
                              ?? "mongodb://localhost:27017";
        var databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME") 
                          ?? configuration.GetSection("MongoDB:DatabaseName").Value 
                          ?? "VYAACentralInforDB";

        // Registrar SystemMongoDbContext como Singleton
        services.AddSingleton<SystemMongoDbContext>(sp => new SystemMongoDbContext(connectionString, databaseName));
        
        // Registrar SalesMongoDbContext como Singleton
        services.AddSingleton<SalesMongoDbContext>(sp => new SalesMongoDbContext(connectionString, databaseName));
    }

    private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        // Configuración de JWT Settings usando variables de entorno del archivo .env
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

        // Registrar el servicio de Token
        services.AddTransient<ITokenService, TokenService>();

        // Configuración de Authentication
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
        // Servicio general de inicialización de base de datos para todos los módulos
        services.AddHostedService<DatabaseInitializationService>();
    }

    private static void AddSystemServices(IServiceCollection services)
    {
        // REPOSITORIOS DEL MÓDULO SYSTEM
        services.AddScoped<IUserRepository, UserRepository>();

        // SERVICIOS DEL MÓDULO SYSTEM
        services.AddScoped<IUserService, UserService>();
    }
    
    private static void AddSalesServices(IServiceCollection services)
    {
        // REPOSITORIOS DEL MÓDULO SALES
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IQuotationRepository, QuotationRepository>();
        services.AddScoped<IQuotationFollowupsRepository, QuotationFollowupsRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        
        // SERVICIOS DEL MÓDULO SALES
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IQuotationService, QuotationService>();
        //services.AddScoped<IQuotationFollowupsService, QuotationFollowupsService>();
        //services.AddScoped<ISaleService, SaleService>();
    }
}
