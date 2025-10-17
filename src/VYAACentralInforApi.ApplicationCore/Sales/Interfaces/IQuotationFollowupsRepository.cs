using VYAACentralInforApi.ApplicationCore.Sales.Entities;

namespace VYAACentralInforApi.ApplicationCore.Sales.Interfaces;

// OBSOLETO: Esta interfaz ya no es necesaria.
// Los followups ahora se manejan directamente dentro de IQuotationRepository
// como parte de la entidad Quotation.
[Obsolete("Los followups ahora se manejan directamente en IQuotationRepository")]
public interface IQuotationFollowupsRepository
{
    Task<IEnumerable<QuotationFollowups>> GetAllFollowupsAsync();
    Task<QuotationFollowups?> GetFollowupByIdAsync(string id);
    Task<IEnumerable<QuotationFollowups>> GetFollowupsByUserIdAsync(string userId);
    Task<IEnumerable<QuotationFollowups>> GetFollowupsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<QuotationFollowups> CreateFollowupAsync(QuotationFollowups followup);
    Task<QuotationFollowups> UpdateFollowupAsync(QuotationFollowups followup);
    Task<bool> DeleteFollowupAsync(string id);
    Task<long> GetTotalFollowupsCountAsync();
    Task<long> GetFollowupsCountByUserAsync(string userId);
}