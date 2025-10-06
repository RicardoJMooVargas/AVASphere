using MongoDB.Driver;
using VYAACentralInforApi.ApplicationCore.Sales.Entities;
using VYAACentralInforApi.ApplicationCore.Sales.Interfaces;
using VYAACentralInforApi.Infrastructure.Sales.Data;

namespace VYAACentralInforApi.Infrastructure.Sales.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly IMongoCollection<Customer> _customers;

    public CustomerRepository(SalesMongoDbContext context)
    {
        _customers = context.Customers;
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        var filter = Builders<Customer>.Filter.Eq(c => c.Status, true);
        return await _customers.Find(filter).ToListAsync();
    }

    public async Task<Customer?> GetCustomerByIdAsync(string id)
    {
        var filter = Builders<Customer>.Filter.And(
            Builders<Customer>.Filter.Eq(c => c.CustomerId, id),
            Builders<Customer>.Filter.Eq(c => c.Status, true)
        );
        return await _customers.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<Customer?> GetCustomerByEmailAsync(string email)
    {
        var filter = Builders<Customer>.Filter.And(
            Builders<Customer>.Filter.Eq(c => c.Email, email),
            Builders<Customer>.Filter.Eq(c => c.Status, true)
        );
        return await _customers.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Customer>> GetCustomersByNameAsync(string name)
    {
        var filter = Builders<Customer>.Filter.And(
            Builders<Customer>.Filter.Regex(c => c.FullName, new MongoDB.Bson.BsonRegularExpression(name, "i")),
            Builders<Customer>.Filter.Eq(c => c.Status, true)
        );
        return await _customers.Find(filter).ToListAsync();
    }

    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        customer.CreatedAt = DateTime.UtcNow;
        customer.Status = true;
        await _customers.InsertOneAsync(customer);
        return customer;
    }

    public async Task<Customer> UpdateCustomerAsync(Customer customer)
    {
        var filter = Builders<Customer>.Filter.Eq(c => c.CustomerId, customer.CustomerId);
        var updateDefinition = Builders<Customer>.Update
            .Set(c => c.FullName, customer.FullName)
            .Set(c => c.Email, customer.Email)
            .Set(c => c.Phones, customer.Phones)
            .Set(c => c.Status, customer.Status);

        var result = await _customers.UpdateOneAsync(filter, updateDefinition);
        
        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException($"Customer with ID {customer.CustomerId} not found.");
        }

        return customer;
    }

    public async Task<bool> DeleteCustomerAsync(string id)
    {
        // Soft delete - cambiar Status a false
        var filter = Builders<Customer>.Filter.Eq(c => c.CustomerId, id);
        var update = Builders<Customer>.Update.Set(c => c.Status, false);
        
        var result = await _customers.UpdateOneAsync(filter, update);
        return result.MatchedCount > 0;
    }

    public async Task<bool> CustomerExistsAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        var filter = Builders<Customer>.Filter.And(
            Builders<Customer>.Filter.Eq(c => c.Email, email),
            Builders<Customer>.Filter.Eq(c => c.Status, true)
        );
        var count = await _customers.CountDocumentsAsync(filter);
        return count > 0;
    }

    public async Task<long> GetTotalCustomersCountAsync()
    {
        var filter = Builders<Customer>.Filter.Eq(c => c.Status, true);
        return await _customers.CountDocumentsAsync(filter);
    }
}