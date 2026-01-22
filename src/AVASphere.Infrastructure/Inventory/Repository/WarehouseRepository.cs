using AVASphere.ApplicationCore.Inventory.Entities.General;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Inventory.Repository;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly MasterDbContext _context;

    public WarehouseRepository(MasterDbContext context)
    {
        _context = context;
    }

    // Create
    public async Task<Warehouse> CreateAsync(Warehouse warehouse)
    {
        if (warehouse is null)
            throw new ArgumentNullException(nameof(warehouse));

        _context.Set<Warehouse>().Add(warehouse);
        await _context.SaveChangesAsync();
        return warehouse;
    }

    // Read
    public async Task<Warehouse?> GetByIdAsync(int idWarehouse)
    {
        return await _context.Set<Warehouse>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.IdWarehouse == idWarehouse);
    }

    public async Task<Warehouse?> GetByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be null or empty.", nameof(code));

        return await _context.Set<Warehouse>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Code == code);
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync()
    {
        return await _context.Set<Warehouse>()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync()
    {
        return await _context.Set<Warehouse>()
            .Where(w => w.Active == 1)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Warehouse?> GetMainWarehouseAsync()
    {
        return await _context.Set<Warehouse>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.IsMain == 1);
    }

    public async Task<bool> ExistsAsync(int idWarehouse)
    {
        return await _context.Set<Warehouse>()
            .AnyAsync(w => w.IdWarehouse == idWarehouse);
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        return await _context.Set<Warehouse>()
            .AnyAsync(w => w.Code == code);
    }

    // Update
    public async Task<Warehouse> UpdateAsync(Warehouse warehouse)
    {
        if (warehouse is null)
            throw new ArgumentNullException(nameof(warehouse));

        var tracked = await _context.Set<Warehouse>().FindAsync(warehouse.IdWarehouse);
        if (tracked == null)
        {
            _context.Set<Warehouse>().Update(warehouse);
        }
        else
        {
            _context.Entry(tracked).CurrentValues.SetValues(warehouse);
        }

        await _context.SaveChangesAsync();
        return warehouse;
    }

    // Delete
    public async Task<bool> DeleteAsync(int idWarehouse)
    {
        var entity = await _context.Set<Warehouse>().FindAsync(idWarehouse);
        if (entity == null)
            return false;

        _context.Set<Warehouse>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
