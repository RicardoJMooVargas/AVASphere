using MongoDB.Driver;
using VYAACentralInforApi.Domain.Sales.Entities;

namespace VYAACentralInforApi.Infrastructure.Sales.Data;

public class SalesMongoDbContext
{
    private readonly IMongoDatabase _database;

    public SalesMongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    // CONTEXTO DEL MÓDULO SALES
    public IMongoCollection<Customer> Customers => _database.GetCollection<Customer>("customers");
    public IMongoCollection<Quotation> Quotations => _database.GetCollection<Quotation>("quotations");
    public IMongoCollection<QuotationFollowups> QuotationFollowups => _database.GetCollection<QuotationFollowups>("quotationFollowups");
    public IMongoCollection<Sale> Sales => _database.GetCollection<Sale>("sales");
    
    // Aquí se pueden agregar más colecciones del módulo Sales cuando las necesites
    // public IMongoCollection<Product> Products => _database.GetCollection<Product>("products");
    // public IMongoCollection<Order> Orders => _database.GetCollection<Order>("orders");
    // public IMongoCollection<Category> Categories => _database.GetCollection<Category>("categories");
}