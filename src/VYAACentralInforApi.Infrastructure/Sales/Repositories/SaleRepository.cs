using MongoDB.Bson;
using MongoDB.Driver;
using VYAACentralInforApi.Domain.Sales.Entities;
using VYAACentralInforApi.Domain.Sales.Interfaces;
using VYAACentralInforApi.Infrastructure.Sales.Data;

namespace VYAACentralInforApi.Infrastructure.Sales.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly IMongoCollection<Sale> _sales;

    public SaleRepository(SalesMongoDbContext context)
    {
        _sales = context.Sales;
    }

    public async Task<IEnumerable<Sale>> GetAllSalesAsync()
    {
        return await _sales.Find(FilterDefinition<Sale>.Empty).ToListAsync();
    }

    public async Task<Sale?> GetSaleByIdAsync(string id)
    {
        var filter = Builders<Sale>.Filter.Eq(s => s.SaleId, id);
        return await _sales.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<Sale?> GetSaleByFolioAsync(string folio)
    {
        var filter = Builders<Sale>.Filter.Eq(s => s.Folio, folio);
        return await _sales.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Sale>> GetSalesByCustomerIdAsync(string customerId)
    {
        var filter = Builders<Sale>.Filter.Eq(s => s.CustomerId, customerId);
        return await _sales.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var filter = Builders<Sale>.Filter.And(
            Builders<Sale>.Filter.Gte(s => s.Date, startDate),
            Builders<Sale>.Filter.Lte(s => s.Date, endDate)
        );
        return await _sales.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetSalesBySalesExecutiveAsync(string salesExecutive)
    {
        var filter = Builders<Sale>.Filter.Eq(s => s.SalesExecutive, salesExecutive);
        return await _sales.Find(filter).ToListAsync();
    }

    public async Task<Sale> CreateSaleAsync(Sale sale)
    {
        sale.Date = DateTime.UtcNow;
        await _sales.InsertOneAsync(sale);
        return sale;
    }

    public async Task<Sale> UpdateSaleAsync(Sale sale)
    {
        var filter = Builders<Sale>.Filter.Eq(s => s.SaleId, sale.SaleId);
        var updateDefinition = Builders<Sale>.Update
            .Set(s => s.SalesExecutive, sale.SalesExecutive)
            .Set(s => s.Type, sale.Type)
            .Set(s => s.CustomerId, sale.CustomerId)
            .Set(s => s.Customer, sale.Customer)
            .Set(s => s.Folio, sale.Folio)
            .Set(s => s.TotalAmount, sale.TotalAmount)
            .Set(s => s.DeliveryDriver, sale.DeliveryDriver)
            .Set(s => s.HomeDelivery, sale.HomeDelivery)
            .Set(s => s.DeliveryDate, sale.DeliveryDate)
            .Set(s => s.SatisfactionLevel, sale.SatisfactionLevel)
            .Set(s => s.SatisfactionReason, sale.SatisfactionReason)
            .Set(s => s.Comment, sale.Comment);

        var result = await _sales.UpdateOneAsync(filter, updateDefinition);
        
        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException($"Sale with ID {sale.SaleId} not found.");
        }

        return sale;
    }

    public async Task<bool> DeleteSaleAsync(string id)
    {
        var filter = Builders<Sale>.Filter.Eq(s => s.SaleId, id);
        var result = await _sales.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<bool> SaleExistsAsync(string folio)
    {
        if (string.IsNullOrEmpty(folio))
            return false;

        var filter = Builders<Sale>.Filter.Eq(s => s.Folio, folio);
        var count = await _sales.CountDocumentsAsync(filter);
        return count > 0;
    }

    public async Task<long> GetTotalSalesCountAsync()
    {
        return await _sales.CountDocumentsAsync(FilterDefinition<Sale>.Empty);
    }

    public async Task<decimal> GetTotalSalesAmountAsync()
    {
        var pipeline = new[]
        {
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "totalAmount", new BsonDocument("$sum", "$totalAmount") }
            })
        };

        var result = await _sales.AggregateAsync<BsonDocument>(pipeline);
        var document = await result.FirstOrDefaultAsync();
        
        if (document != null && document.Contains("totalAmount"))
        {
            return document["totalAmount"].ToDecimal();
        }

        return 0;
    }

    public async Task<decimal> GetTotalSalesAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                { "date", new BsonDocument
                    {
                        { "$gte", startDate },
                        { "$lte", endDate }
                    }
                }
            }),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "totalAmount", new BsonDocument("$sum", "$totalAmount") }
            })
        };

        var result = await _sales.AggregateAsync<BsonDocument>(pipeline);
        var document = await result.FirstOrDefaultAsync();
        
        if (document != null && document.Contains("totalAmount"))
        {
            return document["totalAmount"].ToDecimal();
        }

        return 0;
    }
}
