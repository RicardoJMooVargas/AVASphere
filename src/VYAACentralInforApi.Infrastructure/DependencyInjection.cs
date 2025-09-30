using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VYAACentralInforApi.Application.System.Interfaces;
using VYAACentralInforApi.Application.System.Services;
using VYAACentralInforApi.Domain.System.Interfaces;
using VYAACentralInforApi.Infrastructure.System;

namespace VYAACentralInforApi.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("MongoDB:ConnectionString").Value;
            var databaseName = configuration.GetSection("MongoDB:DatabaseName").Value;

            // Registrar MongoDbContext como Singleton
            services.AddSingleton<MongoDbContext>(sp => new MongoDbContext(connectionString!, databaseName!));

            // REPOSITORIOS DEL MÓDULO SYSTEM
            services.AddScoped<IUserRepository, UserRepository>();

            // SERVICIOS DEL MÓDULO SYSTEM
            services.AddScoped<IUserService, UserService>();

            // Aquí se pueden agregar más repositorios y servicios cuando se implementen otros módulos:
            
            // REPOSITORIOS DEL MÓDULO SALES (futuro)
            // services.AddScoped<ICustomerRepository, CustomerRepository>();
            // services.AddScoped<IProductRepository, ProductRepository>();
            // services.AddScoped<IOrderRepository, OrderRepository>();
            
            // SERVICIOS DEL MÓDULO SALES (futuro)
            // services.AddScoped<ICustomerService, CustomerService>();
            // services.AddScoped<IProductService, ProductService>();
            // services.AddScoped<IOrderService, OrderService>();

            return services;
        }
    }
}
