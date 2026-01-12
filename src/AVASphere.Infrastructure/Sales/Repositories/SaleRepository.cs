using MongoDB.Bson;
using MongoDB.Driver;
using AVASphere.ApplicationCore.Sales.Entities;
using AVASphere.ApplicationCore.Sales.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Sales.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly MasterDbContext _context;

    public SaleRepository(MasterDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Sale>> GetAllSalesAsync()
    {
        return await _context.Set<Sale>()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Sale?> GetSaleByIdAsync(int id)
    {
        return await _context.Set<Sale>()
           .AsNoTracking()
           .FirstOrDefaultAsync(s => s.IdSale == id);
    }

    public async Task<Sale?> GetSaleByFolioAsync(string folio)
    {
        return await _context.Set<Sale>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Folio == folio);
    }

    public async Task<IEnumerable<Sale>> GetSalesByCustomerIdAsync(int customerId)
    {
        return await _context.Set<Sale>()
            .Where(s => s.IdCustomer == customerId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<Sale>()
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Sale>> GetSalesBySalesExecutiveAsync(string salesExecutive)
    {
        return await _context.Set<Sale>()
            .Where(s => s.SalesExecutive == salesExecutive)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Sale> CreateSaleAsync(Sale sale)
    {
        if (sale is null) throw new ArgumentNullException(nameof(sale));
        _context.Set<Sale>().Add(sale);
        await _context.SaveChangesAsync();
        return sale;
    }

    public async Task<Sale> UpdateSaleAsync(Sale sale)
    {
        if (sale is null) throw new ArgumentNullException(nameof(sale));

        var tracked = await _context.Set<Sale>().FindAsync(sale.IdSale);
        if (tracked == null)
        {
            _context.Set<Sale>().Update(sale);
        }
        else
        {
            _context.Entry(tracked).CurrentValues.SetValues(sale);
            tracked.LinkedQuotations = sale.LinkedQuotations;
            tracked.ProductsJson = sale.ProductsJson;
            tracked.AuxNoteDataJson = sale.AuxNoteDataJson;
        }
        await _context.SaveChangesAsync();
        return sale;
    }

    public async Task<bool> DeleteSaleAsync(int id)
    {
        var entity = await _context.Set<Sale>().FindAsync(id);
        if (entity == null) return false;
        _context.Set<Sale>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SaleExistsAsync(string folio)
    {
        return await _context.Set<Sale>().AnyAsync(s => s.Folio == folio);

    }

    public async Task<long> GetTotalSalesCountAsync()
    {
        return await _context.Set<Sale>().LongCountAsync();
    }

    public async Task<decimal> GetTotalSalesAmountAsync()
    {
        return await _context.Set<Sale>().SumAsync(s => s.TotalAmount);
    }

    public async Task<decimal> GetTotalSalesAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<Sale>()
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
            .SumAsync(s => s.TotalAmount);
    }

    // Métodos para importación optimizada
    public async Task<IEnumerable<Sale>> GetSalesByFoliosAsync(IEnumerable<string> folios)
    {
        return await _context.Set<Sale>()
            .Where(s => folios.Contains(s.Folio))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Sale> InsertAsync(Sale sale)
    {
        await _context.Set<Sale>().AddAsync(sale);
        await _context.SaveChangesAsync();
        return sale;
    }
}