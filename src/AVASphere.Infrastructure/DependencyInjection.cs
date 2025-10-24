using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AVASphere.ApplicationCore.System.Interfaces;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.Infrastructure.System.Services;
using AVASphere.Infrastructure.Sales.Data;
using AVASphere.Infrastructure.Sales.Repositories;
using AVASphere.Infrastructure.System.Configuration;
using AVASphere.Infrastructure.System.Data;
using AVASphere.Infrastructure.System.Repositories;
using AVASphere.Infrastructure.Sales.Services;
using AVASphere.Infrastructure.Common.Repositories;
using AVASphere.Infrastructure.Common.Services;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // CONFIGURACIONES DE BASES DE DATOS 
        AddMongoDbConfiguration(services, configuration);
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

    private static void AddMongoDbConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") 
                              ?? configuration.GetSection("MongoDB:ConnectionString").Value 
                              ?? "mongodb://localhost:27017";
        var databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME") 
                          ?? configuration.GetSection("MongoDB:DatabaseName").Value 
                          ?? "VYAACentralInforDB";

        services.AddSingleton<SystemMongoDbContext>(sp => new SystemMongoDbContext(connectionString, databaseName));
        services.AddSingleton<SalesMongoDbContext>(sp => new SalesMongoDbContext(connectionString, databaseName));
    }

    private static void AddPostgreSqlConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
                               ?? configuration.GetSection("DbSettings:ConnectionString").Value
                               ?? "Host=localhost;Port=5432;Database=AVASphereDB;Username=postgres;Password=postgres;";
            
        services.AddDbContext<MasterDbContext>(options =>
            options.UseNpgsql(connectionString));
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
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<DbToolsServices>();
    }

    private static void AddSalesServices(IServiceCollection services)
    {
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IQuotationRepository, QuotationRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IQuotationService, QuotationService>();
    }

    private static void AddCommonServices(IServiceCollection services)
    {
        services.AddScoped<IConfigSysRepository, ConfigSysRepository>();
        services.AddScoped<IConfigSysService, ConfigSysService>();
    }
}