using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Sales.Entities;

namespace AVASphere.ApplicationCore.Sales.Interfaces
{
    public interface IQuotationVersionRepository
    {
        // Lecturas
        Task<IEnumerable<QuotationVersion>> GetAllVersionsAsync();
        Task<QuotationVersion?> GetByIdAsync(int versionId);
        Task<IEnumerable<QuotationVersion>> GetByQuotationIdAsync(int quotationId);
        Task<QuotationVersion?> GetLatestByQuotationIdAsync(int quotationId);

        // Esenciales
        Task<QuotationVersion> CreateAsync(QuotationVersion version);
        Task<QuotationVersion> UpdateAsync(QuotationVersion version);
        Task<bool> DeleteAsync(int versionId);

        // Helpers / checks
        Task<bool> VersionExistsAsync(int versionId);
        Task<int> GetNextVersionNumberAsync(int quotationId);

        // Rango / filtrado (opcional)
        Task<IEnumerable<QuotationVersion>> GetVersionsByDateRangeAsync(DateTime startDate, DateTime endDate);

    }
}