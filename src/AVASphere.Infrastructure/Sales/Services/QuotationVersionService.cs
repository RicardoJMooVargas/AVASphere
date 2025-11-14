using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.Interfaces;

namespace AVASphere.Infrastructure.Sales.Services
{
    public class QuotationVersionService : IQuotationVersionService
    {
        private readonly IQuotationVersionRepository _repo;

        public QuotationVersionService(IQuotationVersionRepository repo)
        {
            _repo = repo;
        }
        // Lectura: GETs públicos
        public async Task<QuotationVersion?> GetVersionAsync(int versionId)
        {
            return await _repo.GetByIdAsync(versionId);
        }

        public async Task<IEnumerable<QuotationVersion>> ListVersionsAsync(int quotationId)
        {
            return await _repo.GetByQuotationIdAsync(quotationId);
        }

        public async Task<QuotationVersion?> GetLatestVersionAsync(int quotationId)
        {
            return await _repo.GetLatestByQuotationIdAsync(quotationId);
        }

        // Utilería (helper)
        public async Task<int> GetNextVersionNumberAsync(int quotationId)
        {
            return await _repo.GetNextVersionNumberAsync(quotationId);
        }

    }
}