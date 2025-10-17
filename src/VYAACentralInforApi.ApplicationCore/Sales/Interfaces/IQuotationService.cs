using VYAACentralInforApi.ApplicationCore.Sales.Entities;

namespace VYAACentralInforApi.ApplicationCore.Sales.Interfaces;

public interface IQuotationService
{
    Task<Quotation> CreateQuotationAsync(Quotation quotation, string createdByUserId);
    Task<IEnumerable<Quotation>> GetQuotationsAsync(DateTime? startDate = null, DateTime? endDate = null, string? customerName = null, int? folio = null);
    Task<Quotation?> GetQuotationByIdAsync(string id);
    Task<Quotation> UpdateQuotationAsync(Quotation quotation);
    Task<bool> DeleteQuotationAsync(string id);
    
    // Métodos para manejar followups dentro de las cotizaciones
    Task<QuotationFollowups> AddFollowupToQuotationAsync(string quotationId, QuotationFollowups followup, string userId);
    Task<QuotationFollowups> UpdateFollowupInQuotationAsync(string quotationId, string followupId, QuotationFollowups updatedFollowup);
    Task<bool> DeleteFollowupFromQuotationAsync(string quotationId, string followupId);
}