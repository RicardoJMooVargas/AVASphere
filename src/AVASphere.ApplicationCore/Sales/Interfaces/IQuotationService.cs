using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Sales.Entities;

namespace AVASphere.ApplicationCore.Sales.Interfaces;

public interface IQuotationService
{
    // Operaciones de negocio
    Task<Quotation> CreateQuotationAsync(CreateQuotationDto createQuotationDto, string createdByUserId);
    Task<IEnumerable<Quotation>> GetQuotationsAsync(DateTime? startDate = null, DateTime? endDate = null, QuotationFilterDto? filter = null);
    Task<Quotation?> UpdateIdQuotation(int IdQuotation, QuotationUpdateDto dto);
    Task<Quotation> GetByIdAsync(int IdQuotation);
    Task<bool> DeleteQuotationAsync(int IdQuotation);

    // Operación para eliminar seguimientos específicos
    Task<bool> DeleteFollowupFromQuotationAsync(int quotationId, int followupId);
}