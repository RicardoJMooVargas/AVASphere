    using AVASphere.ApplicationCore.Inventory.Entities.General;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Inventory.Repository;

public class PhysicalInventoryRepository : IPhysicalInventoryRepository
{
    private readonly MasterDbContext _context;

    public PhysicalInventoryRepository(MasterDbContext context)
    {
        _context = context;
    }

    // Create
    public async Task<PhysicalInventory> CreateAsync(PhysicalInventory physicalInventory)
    {
        if (physicalInventory is null)
            throw new ArgumentNullException(nameof(physicalInventory));

        _context.Set<PhysicalInventory>().Add(physicalInventory);
        await _context.SaveChangesAsync();
        return physicalInventory;
    }

    // Read
    public async Task<PhysicalInventory?> GetByIdAsync(int idPhysicalInventory)
    {
        return await _context.Set<PhysicalInventory>()
            .Include(pi => pi.Warehouse)
            .AsNoTracking()
            .FirstOrDefaultAsync(pi => pi.IdPhysicalInventory == idPhysicalInventory);
    }

    public async Task<PhysicalInventory?> GetByIdWithDetailsAsync(int idPhysicalInventory)
    {
        return await _context.Set<PhysicalInventory>()
            .Include(pi => pi.Warehouse)
            .Include(pi => pi.PhysicalInventoryDetails)
                .ThenInclude(pid => pid.Product)
            .Include(pi => pi.PhysicalInventoryDetails)
                .ThenInclude(pid => pid.LocationDetails)
            .AsNoTracking()
            .FirstOrDefaultAsync(pi => pi.IdPhysicalInventory == idPhysicalInventory);
    }

    public async Task<IEnumerable<PhysicalInventory>> GetAllAsync()
    {
        return await _context.Set<PhysicalInventory>()
            .Include(pi => pi.Warehouse)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<PhysicalInventory>> GetByWarehouseIdAsync(int idWarehouse)
    {
        return await _context.Set<PhysicalInventory>()
            .Where(pi => pi.IdWarehouse == idWarehouse)
            .Include(pi => pi.Warehouse)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<PhysicalInventory>> GetByStatusAsync(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status cannot be null or empty.", nameof(status));

        return await _context.Set<PhysicalInventory>()
            .Where(pi => pi.Status == status)
            .Include(pi => pi.Warehouse)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<PhysicalInventory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<PhysicalInventory>()
            .Where(pi => pi.InventoryDate >= startDate && pi.InventoryDate <= endDate)
            .Include(pi => pi.Warehouse)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int idPhysicalInventory)
    {
        return await _context.Set<PhysicalInventory>()
            .AnyAsync(pi => pi.IdPhysicalInventory == idPhysicalInventory);
    }

    // Update
    public async Task<PhysicalInventory> UpdateAsync(PhysicalInventory physicalInventory)
    {
        if (physicalInventory is null)
            throw new ArgumentNullException(nameof(physicalInventory));

        var tracked = await _context.Set<PhysicalInventory>().FindAsync(physicalInventory.IdPhysicalInventory);
        if (tracked == null)
        {
            _context.Set<PhysicalInventory>().Update(physicalInventory);
        }
        else
        {
            _context.Entry(tracked).CurrentValues.SetValues(physicalInventory);
        }

        await _context.SaveChangesAsync();
        return physicalInventory;
    }

    public async Task<bool> UpdateStatusAsync(int idPhysicalInventory, string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status cannot be null or empty.", nameof(status));

        var physicalInventory = await _context.Set<PhysicalInventory>().FindAsync(idPhysicalInventory);
        if (physicalInventory == null)
            return false;

        physicalInventory.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }

    // Delete
    public async Task<bool> DeleteAsync(int idPhysicalInventory)
    {
        var entity = await _context.Set<PhysicalInventory>().FindAsync(idPhysicalInventory);
        if (entity == null)
            return false;

        _context.Set<PhysicalInventory>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
