using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using AVASphere.ApplicationCore.Sales.Entities;

namespace AVASphere.ApplicationCore.Sales.Interfaces
{
    public interface IQuotationRepository
    {
        Task<IEnumerable<Quotation>> GetAllQuotationsAsync();
        Task<Quotation?> GetQuotationByIdAsync(string id);
        Task<Quotation?> GetQuotationByFolioAsync(int folio);
        Task<IEnumerable<Quotation>> GetQuotationsByCustomerIdAsync(string customerId);
        Task<IEnumerable<Quotation>> GetQuotationsByStatusAsync(string status);
        Task<IEnumerable<Quotation>> GetQuotationsBySalesExecutiveAsync(string salesExecutiveId);
        Task<IEnumerable<Quotation>> GetQuotationsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Quotation> CreateQuotationAsync(Quotation quotation);
        Task<Quotation> UpdateQuotationAsync(Quotation quotation);
        Task<bool> DeleteQuotationAsync(string id);
        Task<bool> QuotationExistsByFolioAsync(int folio);
        Task<int> GetNextFolioAsync();
        Task<long> GetTotalQuotationsCountAsync();
        Task<Quotation> AddFollowupAsync(string quotationId, QuotationFollowups followup);
        Task<bool> UpdateQuotationStatusAsync(string quotationId, string status);
    }
}
