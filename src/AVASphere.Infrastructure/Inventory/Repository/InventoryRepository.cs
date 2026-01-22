using InventoryEntity = AVASphere.ApplicationCore.Inventory.Entities.General.Inventory;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Inventory.Repository;

public class InventoryRepository : IInventoryRepository
{
    private readonly MasterDbContext _context;

    public InventoryRepository(MasterDbContext context)
    {
        _context = context;
    }

    // Create
    public async Task<InventoryEntity> CreateAsync(InventoryEntity inventory)
    {
        if (inventory is null)
            throw new ArgumentNullException(nameof(inventory));

        _context.Set<InventoryEntity>().Add(inventory);
        await _context.SaveChangesAsync();
        return inventory;
    }

    // Read
    public async Task<InventoryEntity?> GetByIdAsync(int idInventory)
    {
        return await _context.Set<InventoryEntity>()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Include(i => i.PhysicalInventory)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.IdInventory == idInventory);
    }

    public async Task<IEnumerable<InventoryEntity>> GetAllAsync()
    {
        return await _context.Set<InventoryEntity>()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryEntity>> GetByWarehouseIdAsync(int idWarehouse)
    {
        return await _context.Set<InventoryEntity>()
            .Where(i => i.IdWarehouse == idWarehouse)
            .Include(i => i.Product)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryEntity>> GetByProductIdAsync(int idProduct)
    {
        return await _context.Set<InventoryEntity>()
            .Where(i => i.IdProduct == idProduct)
            .Include(i => i.Warehouse)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<InventoryEntity?> GetByWarehouseAndProductAsync(int idWarehouse, int idProduct)
    {
        return await _context.Set<InventoryEntity>()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.IdWarehouse == idWarehouse && i.IdProduct == idProduct);
    }

    public async Task<IEnumerable<InventoryEntity>> GetLowStockItemsAsync()
    {
        return await _context.Set<InventoryEntity>()
            .Where(i => i.Stock <= i.StockMin)
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int idInventory)
    {
        return await _context.Set<InventoryEntity>()
            .AnyAsync(i => i.IdInventory == idInventory);
    }

    // Update
    public async Task<InventoryEntity> UpdateAsync(InventoryEntity inventory)
    {
        if (inventory is null)
            throw new ArgumentNullException(nameof(inventory));

        var tracked = await _context.Set<InventoryEntity>().FindAsync(inventory.IdInventory);
        if (tracked == null)
        {
            _context.Set<InventoryEntity>().Update(inventory);
        }
        else
        {
            _context.Entry(tracked).CurrentValues.SetValues(inventory);
        }

        await _context.SaveChangesAsync();
        return inventory;
    }

    public async Task<bool> UpdateStockAsync(int idInventory, double newStock)
    {
        var inventory = await _context.Set<InventoryEntity>().FindAsync(idInventory);
        if (inventory == null)
            return false;

        inventory.Stock = newStock;
        await _context.SaveChangesAsync();
        return true;
    }

    // Delete
    public async Task<bool> DeleteAsync(int idInventory)
    {
        var entity = await _context.Set<InventoryEntity>().FindAsync(idInventory);
        if (entity == null)
            return false;

        _context.Set<InventoryEntity>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
