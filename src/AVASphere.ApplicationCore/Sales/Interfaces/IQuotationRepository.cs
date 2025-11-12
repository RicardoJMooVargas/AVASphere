using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.DTOs;

namespace AVASphere.ApplicationCore.Sales.Interfaces
{
    public interface IQuotationRepository
    {
        // Lecturas
        Task<IEnumerable<Quotation>> GetAllQuotationsAsync();
        Task<Quotation?> GetByIdAsync(int id);
        Task<Quotation?> UpdateIdQuotation(int id, QuotationUpdateDto dto);
        Task<Quotation> UpdateQuotationAsync(Quotation quotation);
        Task<Quotation?> GetQuotationByFolioAsync(int folio);
        Task<IEnumerable<Quotation>> GetQuotationsByCustomerIdAsync(int customerId);
        Task<IEnumerable<Quotation>> GetQuotationsByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Esenciales
        Task<Quotation> CreateQuotationAsync(Quotation quotation);
        Task<bool> DeleteQuotationAsync(int id);
        // Helpers / checks
        Task<bool> QuotationExistsByFolioAsync(int folio);

        // Followups
        Task<int> GetNextFollowupIdAsync(int quotationId);
    }
}
