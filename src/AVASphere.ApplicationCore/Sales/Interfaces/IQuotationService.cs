using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Sales.Entities;

namespace AVASphere.ApplicationCore.Sales.Interfaces;

public interface IQuotationService
{
    // Operaciones de negocio (usualmente reciben DTOs; aquí muestro con entidades por simplicidad)
    Task<Quotation> CreateQuotationAsync(CreateQuotationDto createQuotationDto, string createdByUserId);
    Task<IEnumerable<Quotation>> GetQuotationsAsync(DateTime? startDate = null, DateTime? endDate = null, string? customerName = null, int? folio = null);
    Task<Quotation?> UpdateIdQuotation(int IdQuotation, QuotationUpdateDto dto);
    Task<Quotation> GetByIdAsync(int IdQuotation);
    Task<bool> DeleteQuotationAsync(int IdQuotation);

    // Reglas de negocio para FollowupsJson (maneja validaciones y auditable)
    Task<bool> AddFollowupAsync(int quotationId, QuotationFollowupsJson followup, string userId);
    Task<bool> DeleteFollowupFromQuotationAsync(int quotationId, int followupId);
}