using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Sales.Entities;

namespace AVASphere.ApplicationCore.Sales.Interfaces
{
    public interface IQuotationVersionService
    {
        // SOLO operaciones de lectura / utilerías (la creación es responsabilidad de QuotationService)
        Task<QuotationVersion?> GetVersionAsync(int versionId);
        Task<IEnumerable<QuotationVersion>> ListVersionsAsync(int quotationId);
        Task<QuotationVersion?> GetLatestVersionAsync(int quotationId);

        // Utilerías de negocio
        Task<int> GetNextVersionNumberAsync(int quotationId);
    }
}