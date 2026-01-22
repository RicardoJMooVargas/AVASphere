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
