using MongoDB.Driver;
using VYAACentralInforApi.ApplicationCore.System;

namespace VYAACentralInforApi.Infrastructure.System.Data
{
    public class SystemMongoDbContext
    {
        private readonly IMongoDatabase _database;

        public SystemMongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        // CONTEXTO DEL SISTEMA
        public IMongoCollection<Users> Users => _database.GetCollection<Users>("users");
        
        // Aquí se pueden agregar más colecciones del módulo System cuando las necesites
        // public IMongoCollection<Roles> Roles => _database.GetCollection<Roles>("roles");
        // public IMongoCollection<Permissions> Permissions => _database.GetCollection<Permissions>("permissions");
        
        // CONTEXTO DE VENTAS (para futuro uso)
        // public IMongoCollection<Customer> Customers => _database.GetCollection<Customer>("customers");
        // public IMongoCollection<Product> Products => _database.GetCollection<Product>("products");
        // public IMongoCollection<Order> Orders => _database.GetCollection<Order>("orders");
    }
}