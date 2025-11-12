using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Sales.Entities;

namespace AVASphere.ApplicationCore.Sales.Interfaces
{
    public interface ISaleQuotationRepository
    {
        // Lecturas
        Task<IEnumerable<SaleQuotation>> GetAllAsync();
        Task<SaleQuotation?> GetByIdAsync(int id); // o composite key si aplica
        Task<IEnumerable<SaleQuotation>> GetBySaleIdAsync(int saleId);
        Task<IEnumerable<SaleQuotation>> GetByQuotationIdAsync(int quotationId);

        // Esenciales
        Task<SaleQuotation> CreateAsync(SaleQuotation saleQuotation);
        Task<bool> DeleteAsync(int saleId, int quotationId);

        // Helpers / checks
        Task<bool> LinkExistsAsync(int saleId, int quotationId);
        Task<SaleQuotation?> GetPrimaryForSaleAsync(int saleId);
    }
}