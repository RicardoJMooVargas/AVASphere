using MongoDB.Driver;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.Infrastructure.Sales.Data;

namespace AVASphere.Infrastructure.Sales.Repositories;

public class QuotationRepository : IQuotationRepository
{
    private readonly IMongoCollection<Quotation> _quotations;

    public QuotationRepository(SalesMongoDbContext context)
    {
        _quotations = context.Quotations;
    }

    public async Task<IEnumerable<Quotation>> GetAllQuotationsAsync()
    {
        return await _quotations.Find(_ => true).ToListAsync();
    }

    public async Task<Quotation?> GetQuotationByIdAsync(string id)
    {
        var filter = Builders<Quotation>.Filter.Eq(q => q.QuotationId, id);
        return await _quotations.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<Quotation?> GetQuotationByFolioAsync(int folio)
    {
        var filter = Builders<Quotation>.Filter.Eq(q => q.Folio, folio);
        return await _quotations.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Quotation>> GetQuotationsByCustomerIdAsync(string customerId)
    {
        var filter = Builders<Quotation>.Filter.Eq(q => q.CustomerId, customerId);
        return await _quotations.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<Quotation>> GetQuotationsByStatusAsync(string status)
    {
        var filter = Builders<Quotation>.Filter.Eq(q => q.Status, status);
        return await _quotations.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<Quotation>> GetQuotationsBySalesExecutiveAsync(string salesExecutiveId)
    {
        var filter = Builders<Quotation>.Filter.AnyEq(q => q.SalesExecutives, salesExecutiveId);
        return await _quotations.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<Quotation>> GetQuotationsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var filter = Builders<Quotation>.Filter.And(
            Builders<Quotation>.Filter.Gte(q => q.SaleDate, startDate),
            Builders<Quotation>.Filter.Lte(q => q.SaleDate, endDate)
        );
        return await _quotations.Find(filter).ToListAsync();
    }

    public async Task<Quotation> CreateQuotationAsync(Quotation quotation)
    {
        // Solo verificar que no existe una cotización con el mismo folio
        if (await QuotationExistsByFolioAsync(quotation.Folio))
        {
            throw new InvalidOperationException($"A quotation with folio {quotation.Folio} already exists.");
        }

        quotation.CreatedAt = DateTime.UtcNow;
        quotation.UpdatedAt = DateTime.UtcNow;
        
        await _quotations.InsertOneAsync(quotation);
        return quotation;
    }

    public async Task<Quotation> UpdateQuotationAsync(Quotation quotation)
    {
        // Verificar que la cotización existe
        var existingQuotation = await GetQuotationByIdAsync(quotation.QuotationId);
        if (existingQuotation == null)
        {
            throw new InvalidOperationException($"Quotation with ID {quotation.QuotationId} not found.");
        }


        quotation.UpdatedAt = DateTime.UtcNow;
        
        var filter = Builders<Quotation>.Filter.Eq(q => q.QuotationId, quotation.QuotationId);
        var result = await _quotations.ReplaceOneAsync(filter, quotation);
        
        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException($"Quotation with ID {quotation.QuotationId} not found.");
        }

        return quotation;
    }

    public async Task<bool> DeleteQuotationAsync(string id)
    {
        var filter = Builders<Quotation>.Filter.Eq(q => q.QuotationId, id);
        var result = await _quotations.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<bool> QuotationExistsByFolioAsync(int folio)
    {
        var filter = Builders<Quotation>.Filter.Eq(q => q.Folio, folio);
        var count = await _quotations.CountDocumentsAsync(filter);
        return count > 0;
    }

    public async Task<int> GetNextFolioAsync()
    {
        var lastQuotation = await _quotations
            .Find(_ => true)
            .SortByDescending(q => q.Folio)
            .FirstOrDefaultAsync();

        return lastQuotation?.Folio + 1 ?? 1;
    }

    public async Task<long> GetTotalQuotationsCountAsync()
    {
        return await _quotations.CountDocumentsAsync(_ => true);
    }

    public async Task<Quotation> AddFollowupAsync(string quotationId, QuotationFollowups followup)
    {
        var quotation = await GetQuotationByIdAsync(quotationId);
        if (quotation == null)
        {
            throw new InvalidOperationException($"Quotation with ID {quotationId} not found.");
        }

        followup.CreatedAt = DateTime.UtcNow;
        
        var filter = Builders<Quotation>.Filter.Eq(q => q.QuotationId, quotationId);
        var update = Builders<Quotation>.Update
            .Push(q => q.Followups, followup)
            .Set(q => q.UpdatedAt, DateTime.UtcNow);

        var result = await _quotations.UpdateOneAsync(filter, update);
        
        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException($"Quotation with ID {quotationId} not found.");
        }

        return await GetQuotationByIdAsync(quotationId) ?? throw new InvalidOperationException("Failed to retrieve updated quotation.");
    }

    public async Task<bool> UpdateQuotationStatusAsync(string quotationId, string status)
    {
        var validStatuses = new[] { "Pending", "Accepted", "Rejected" };
        if (!validStatuses.Contains(status))
        {
            throw new ArgumentException($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");
        }

        var filter = Builders<Quotation>.Filter.Eq(q => q.QuotationId, quotationId);
        var update = Builders<Quotation>.Update
            .Set(q => q.Status, status)
            .Set(q => q.UpdatedAt, DateTime.UtcNow);

        var result = await _quotations.UpdateOneAsync(filter, update);
        return result.MatchedCount > 0;
    }
}