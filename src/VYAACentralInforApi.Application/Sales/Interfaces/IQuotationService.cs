using VYAACentralInforApi.Domain.Sales.Entities;

namespace VYAACentralInforApi.Application.Sales.Interfaces;

public interface IQuotationService
{
    Task<Quotation> CreateQuotationAsync(Quotation quotation, string createdByUserId);
    Task<IEnumerable<Quotation>> GetQuotationsAsync(DateTime? startDate = null, DateTime? endDate = null, string? customerName = null, int? folio = null);
    Task<Quotation?> GetQuotationByIdAsync(string id);
    Task<Quotation> UpdateQuotationAsync(Quotation quotation);
    Task<bool> DeleteQuotationAsync(string id);
}