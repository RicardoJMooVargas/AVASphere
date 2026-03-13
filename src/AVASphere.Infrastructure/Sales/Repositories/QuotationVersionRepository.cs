using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.Interfaces;
using Microsoft.EntityFrameworkCore;
using AVASphere.Infrastructure;

namespace AVASphere.Infrastructure.Sales.Repositories
{
    public class QuotationVersionRepository : IQuotationVersionRepository
    {
        private readonly MasterDbContext _context;

        public QuotationVersionRepository(MasterDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<QuotationVersion>> GetAllVersionsAsync()
        {
            return await _context.Set<QuotationVersion>().AsNoTracking().ToListAsync();
        }

        public async Task<QuotationVersion?> GetByIdAsync(int versionId)
        {
            return await _context.Set<QuotationVersion>().FindAsync(versionId);
        }

        public async Task<IEnumerable<QuotationVersion>> GetByQuotationIdAsync(int quotationId)
        {
            return await _context.Set<QuotationVersion>()
                .Where(v => v.IdQuotation == quotationId)
                .AsNoTracking()
                .OrderByDescending(v => v.VersionNumber)
                .ToListAsync();
        }

        public async Task<QuotationVersion?> GetLatestByQuotationIdAsync(int quotationId)
        {
            return await _context.Set<QuotationVersion>()
                .Where(v => v.IdQuotation == quotationId)
                .OrderByDescending(v => v.VersionNumber)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<QuotationVersion> CreateAsync(QuotationVersion version)
        {
            await _context.Set<QuotationVersion>().AddAsync(version);
            await _context.SaveChangesAsync();
            return version;
        }

        public async Task<QuotationVersion> UpdateAsync(QuotationVersion version)
        {
            _context.Set<QuotationVersion>().Update(version);
            await _context.SaveChangesAsync();
            return version;
        }

        public async Task<bool> DeleteAsync(int versionId)
        {
            var entity = await _context.Set<QuotationVersion>().FindAsync(versionId);
            if (entity == null) return false;
            _context.Set<QuotationVersion>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VersionExistsAsync(int versionId)
        {
            return await _context.Set<QuotationVersion>().AsNoTracking().AnyAsync(v => v.IdQuotationVersion == versionId);
        }

        public async Task<int> GetNextVersionNumberAsync(int quotationId)
        {
            var max = await _context.Set<QuotationVersion>()
                .Where(v => v.IdQuotation == quotationId)
                .MaxAsync(v => (int?)v.VersionNumber);
            return (max ?? 0) + 1;
        }

        public async Task<IEnumerable<QuotationVersion>> GetVersionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Set<QuotationVersion>()
                .Where(v => v.CreatedAt >= startDate && v.CreatedAt <= endDate)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}