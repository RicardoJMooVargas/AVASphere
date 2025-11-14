using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.Interfaces;
using AVASphere.Infrastructure;

namespace AVASphere.Infrastructure.Sales.Repositories
{
    public class SaleQuotationRepository : ISaleQuotationRepository
    {
        private readonly MasterDbContext _context;

        public SaleQuotationRepository(MasterDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SaleQuotation>> GetAllAsync()
        {
            return await _context.Set<SaleQuotation>().AsNoTracking().ToListAsync();
        }

        public async Task<SaleQuotation?> GetByIdAsync(int id)
        {
            var found = await _context.Set<SaleQuotation>().FindAsync(id);
            if (found != null) return found;
            return null;
        }

        public async Task<IEnumerable<SaleQuotation>> GetBySaleIdAsync(int saleId)
        {
            return await _context.Set<SaleQuotation>()
                .Where(sq => sq.IdSale == saleId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<SaleQuotation>> GetByQuotationIdAsync(int quotationId)
        {
            return await _context.Set<SaleQuotation>()
                .Where(sq => sq.IdQuotation == quotationId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<SaleQuotation> CreateAsync(SaleQuotation saleQuotation)
        {
            await _context.Set<SaleQuotation>().AddAsync(saleQuotation);
            await _context.SaveChangesAsync();
            return saleQuotation;
        }

        public async Task<bool> DeleteAsync(int saleId, int quotationId)
        {
            var entity = await _context.Set<SaleQuotation>().FindAsync(saleId, quotationId);
            if (entity == null) return false;
            _context.Set<SaleQuotation>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LinkExistsAsync(int saleId, int quotationId)
        {
            return await _context.Set<SaleQuotation>().AsNoTracking().AnyAsync(sq => sq.IdSale == saleId && sq.IdQuotation == quotationId);
        }

        public async Task<SaleQuotation?> GetPrimaryForSaleAsync(int saleId)
        {
            return await _context.Set<SaleQuotation>()
                .Where(sq => sq.IdSale == saleId)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<SaleQuotation> UpdateAsync(SaleQuotation saleQuotation)
        {
            _context.Set<SaleQuotation>().Update(saleQuotation);
            await _context.SaveChangesAsync();
            return saleQuotation;
        }
    }
}