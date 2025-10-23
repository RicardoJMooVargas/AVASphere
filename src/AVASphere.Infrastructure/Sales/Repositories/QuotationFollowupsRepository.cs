using MongoDB.Driver;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.Infrastructure.Sales.Data;

namespace AVASphere.Infrastructure.Sales.Repositories;

// OBSOLETO: Este repositorio ya no es necesario.
// Los followups ahora se manejan directamente dentro de QuotationRepository
// como parte de la entidad Quotation.
[Obsolete("Los followups ahora se manejan directamente en QuotationRepository")]
public class QuotationFollowupsRepository : IQuotationFollowupsRepository
{
    // Este repositorio puede ser eliminado
    // Los métodos han sido movidos a QuotationRepository como parte de la entidad Quotation
       
    private readonly IMongoCollection<QuotationFollowups> _followups;

    public QuotationFollowupsRepository(SalesMongoDbContext context)
    {
        //_followups = context.QuotationFollowups;
    }

    public async Task<IEnumerable<QuotationFollowups>> GetAllFollowupsAsync()
    {
        return await _followups.Find(FilterDefinition<QuotationFollowups>.Empty)
                              .SortByDescending(f => f.CreatedAt)
                              .ToListAsync();
    }

    public async Task<QuotationFollowups?> GetFollowupByIdAsync(string id)
    {
        var filter = Builders<QuotationFollowups>.Filter.Eq(f => f.Id, id);
        return await _followups.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<QuotationFollowups>> GetFollowupsByUserIdAsync(string userId)
    {
        var filter = Builders<QuotationFollowups>.Filter.Eq(f => f.UserId, userId);
        return await _followups.Find(filter)
                              .SortByDescending(f => f.CreatedAt)
                              .ToListAsync();
    }

    public async Task<IEnumerable<QuotationFollowups>> GetFollowupsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var filter = Builders<QuotationFollowups>.Filter.And(
            Builders<QuotationFollowups>.Filter.Gte(f => f.Date, startDate),
            Builders<QuotationFollowups>.Filter.Lte(f => f.Date, endDate)
        );
        return await _followups.Find(filter)
                              .SortByDescending(f => f.Date)
                              .ToListAsync();
    }

    public async Task<QuotationFollowups> CreateFollowupAsync(QuotationFollowups followup)
    {
        followup.Date = DateTime.UtcNow;
        followup.CreatedAt = DateTime.UtcNow;
        await _followups.InsertOneAsync(followup);
        return followup;
    }

    public async Task<QuotationFollowups> UpdateFollowupAsync(QuotationFollowups followup)
    {
        var filter = Builders<QuotationFollowups>.Filter.Eq(f => f.Id, followup.Id);
        var updateDefinition = Builders<QuotationFollowups>.Update
            .Set(f => f.Date, followup.Date)
            .Set(f => f.Comment, followup.Comment)
            .Set(f => f.UserId, followup.UserId);

        var result = await _followups.UpdateOneAsync(filter, updateDefinition);
        
        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException($"QuotationFollowup with ID {followup.Id} not found.");
        }

        return followup;
    }

    public async Task<bool> DeleteFollowupAsync(string id)
    {
        var filter = Builders<QuotationFollowups>.Filter.Eq(f => f.Id, id);
        var result = await _followups.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<long> GetTotalFollowupsCountAsync()
    {
        return await _followups.CountDocumentsAsync(FilterDefinition<QuotationFollowups>.Empty);
    }

    public async Task<long> GetFollowupsCountByUserAsync(string userId)
    {
        var filter = Builders<QuotationFollowups>.Filter.Eq(f => f.UserId, userId);
        return await _followups.CountDocumentsAsync(filter);
    }
}