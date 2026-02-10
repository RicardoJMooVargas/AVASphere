using AVASphere.ApplicationCore.Inventory.Entities.General;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Inventory.Repository;

public class PhysicalInventoryDetailRepository : IPhysicalInventoryDetailRepository
{
    private readonly MasterDbContext _context;

    public PhysicalInventoryDetailRepository(MasterDbContext context)
    {
        _context = context;
    }

    // Create
    public async Task<PhysicalInventoryDetail> CreateAsync(PhysicalInventoryDetail detail)
    {
        if (detail is null)
            throw new ArgumentNullException(nameof(detail));

        _context.Set<PhysicalInventoryDetail>().Add(detail);
        await _context.SaveChangesAsync();
        return detail;
    }

    public async Task<IEnumerable<PhysicalInventoryDetail>> CreateBatchAsync(IEnumerable<PhysicalInventoryDetail> details)
    {
        if (details is null || !details.Any())
            throw new ArgumentException("Details collection cannot be null or empty.", nameof(details));

        _context.Set<PhysicalInventoryDetail>().AddRange(details);
        await _context.SaveChangesAsync();
        return details;
    }

    // Read
    public async Task<PhysicalInventoryDetail?> GetByIdAsync(int idPhysicalInventoryDetail)
    {
        return await _context.Set<PhysicalInventoryDetail>()
            .Include(pid => pid.PhysicalInventory)
            .Include(pid => pid.Product)
            .Include(pid => pid.LocationDetails)
            .AsNoTracking()
            .FirstOrDefaultAsync(pid => pid.IdPhysicalInventoryDetail == idPhysicalInventoryDetail);
    }

    public async Task<IEnumerable<PhysicalInventoryDetail>> GetAllAsync()
    {
        return await _context.Set<PhysicalInventoryDetail>()
            .Include(pid => pid.Product)
            .Include(pid => pid.LocationDetails)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<PhysicalInventoryDetail>> GetByPhysicalInventoryIdAsync(int idPhysicalInventory)
    {
        return await _context.Set<PhysicalInventoryDetail>()
            .Where(pid => pid.IdPhysicalInventory == idPhysicalInventory)
            .Include(pid => pid.Product)
            .Include(pid => pid.LocationDetails)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<PhysicalInventoryDetail>> GetByProductIdAsync(int idProduct)
    {
        return await _context.Set<PhysicalInventoryDetail>()
            .Where(pid => pid.IdProduct == idProduct)
            .Include(pid => pid.PhysicalInventory)
            .Include(pid => pid.LocationDetails)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<PhysicalInventoryDetail>> GetDiscrepanciesAsync(int idPhysicalInventory)
    {
        return await _context.Set<PhysicalInventoryDetail>()
            .Where(pid => pid.IdPhysicalInventory == idPhysicalInventory && pid.Difference != 0)
            .Include(pid => pid.Product)
            .Include(pid => pid.LocationDetails)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<PhysicalInventoryDetail>> GetFilteredAsync(
        int? idPhysicalInventoryDetail = null,
        double? minSystemQuantity = null,
        double? maxSystemQuantity = null,
        double? minPhysicalQuantity = null,
        double? maxPhysicalQuantity = null,
        double? minDifference = null,
        double? maxDifference = null,
        int? idPhysicalInventory = null,
        int? idProduct = null,
        int? idLocationDetails = null,
        bool? hasDifferences = null)
    {
        var query = _context.Set<PhysicalInventoryDetail>()
            .Include(pid => pid.PhysicalInventory)
            .Include(pid => pid.Product)
            .Include(pid => pid.LocationDetails)
            .AsQueryable();

        if (idPhysicalInventoryDetail.HasValue)
            query = query.Where(pid => pid.IdPhysicalInventoryDetail == idPhysicalInventoryDetail.Value);

        if (minSystemQuantity.HasValue)
            query = query.Where(pid => pid.SystemQuantity >= minSystemQuantity.Value);

        if (maxSystemQuantity.HasValue)
            query = query.Where(pid => pid.SystemQuantity <= maxSystemQuantity.Value);

        if (minPhysicalQuantity.HasValue)
            query = query.Where(pid => pid.PhysicalQuantity >= minPhysicalQuantity.Value);

        if (maxPhysicalQuantity.HasValue)
            query = query.Where(pid => pid.PhysicalQuantity <= maxPhysicalQuantity.Value);

        if (minDifference.HasValue)
            query = query.Where(pid => pid.Difference >= minDifference.Value);

        if (maxDifference.HasValue)
            query = query.Where(pid => pid.Difference <= maxDifference.Value);

        if (idPhysicalInventory.HasValue)
            query = query.Where(pid => pid.IdPhysicalInventory == idPhysicalInventory.Value);

        if (idProduct.HasValue)
            query = query.Where(pid => pid.IdProduct == idProduct.Value);

        if (idLocationDetails.HasValue)
            query = query.Where(pid => pid.IdLocationDetails == idLocationDetails.Value);

        if (hasDifferences.HasValue)
        {
            if (hasDifferences.Value)
                query = query.Where(pid => pid.Difference != 0);
            else
                query = query.Where(pid => pid.Difference == 0);
        }

        return await query
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int idPhysicalInventoryDetail)
    {
        return await _context.Set<PhysicalInventoryDetail>()
            .AnyAsync(pid => pid.IdPhysicalInventoryDetail == idPhysicalInventoryDetail);
    }

    // Update
    public async Task<PhysicalInventoryDetail> UpdateAsync(PhysicalInventoryDetail detail)
    {
        if (detail is null)
            throw new ArgumentNullException(nameof(detail));

        var tracked = await _context.Set<PhysicalInventoryDetail>().FindAsync(detail.IdPhysicalInventoryDetail);
        if (tracked == null)
        {
            _context.Set<PhysicalInventoryDetail>().Update(detail);
        }
        else
        {
            _context.Entry(tracked).CurrentValues.SetValues(detail);
        }

        await _context.SaveChangesAsync();
        return detail;
    }

    // Delete
    public async Task<bool> DeleteAsync(int idPhysicalInventoryDetail)
    {
        var entity = await _context.Set<PhysicalInventoryDetail>().FindAsync(idPhysicalInventoryDetail);
        if (entity == null)
            return false;

        _context.Set<PhysicalInventoryDetail>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteByPhysicalInventoryIdAsync(int idPhysicalInventory)
    {
        var details = await _context.Set<PhysicalInventoryDetail>()
            .Where(pid => pid.IdPhysicalInventory == idPhysicalInventory)
            .ToListAsync();

        if (!details.Any())
            return false;

        _context.Set<PhysicalInventoryDetail>().RemoveRange(details);
        await _context.SaveChangesAsync();
        return true;
    }
}
