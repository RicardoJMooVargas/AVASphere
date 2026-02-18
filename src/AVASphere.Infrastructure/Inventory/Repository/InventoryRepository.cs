using AVASphere.ApplicationCore.Inventory.Interfaces;
using InventoryEntity = AVASphere.ApplicationCore.Inventory.Entities.General.Inventory;
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
        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync();
        return inventory;
    }

    // Read
    public async Task<InventoryEntity?> GetByIdAsync(int idInventory)
    {
        return await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Include(i => i.PhysicalInventory)
            .FirstOrDefaultAsync(i => i.IdInventory == idInventory);
    }

    public async Task<IEnumerable<InventoryEntity>> GetAllAsync()
    {
        return await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Include(i => i.PhysicalInventory)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryEntity>> GetByWarehouseIdAsync(int idWarehouse)
    {
        return await _context.Inventories
            .Include(i => i.Product)
                .ThenInclude(p => p.Supplier)
            .Include(i => i.Product)
                .ThenInclude(p => p.ProductProperties)
                    .ThenInclude(pp => pp.PropertyValue)
                        .ThenInclude(pv => pv.Property)
            .Include(i => i.Warehouse)
            .Include(i => i.PhysicalInventory)
            .Where(i => i.IdWarehouse == idWarehouse)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryEntity>> GetByProductIdAsync(int idProduct)
    {
        return await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Include(i => i.PhysicalInventory)
            .Where(i => i.IdProduct == idProduct)
            .ToListAsync();
    }

    public async Task<InventoryEntity?> GetByWarehouseAndProductAsync(int idWarehouse, int idProduct)
    {
        return await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Include(i => i.PhysicalInventory)
            .FirstOrDefaultAsync(i => i.IdWarehouse == idWarehouse && i.IdProduct == idProduct);
    }

    public async Task<IEnumerable<InventoryEntity>> GetLowStockItemsAsync()
    {
        return await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Include(i => i.PhysicalInventory)
            .Where(i => i.Stock <= i.StockMin)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryEntity>> GetFilteredAsync(
        int? idInventory = null,
        double? minStock = null,
        double? maxStock = null,
        double? minStockMin = null,
        double? maxStockMin = null,
        double? minStockMax = null,
        double? maxStockMax = null,
        double? locationDetail = null,
        int? idPhysicalInventory = null,
        int? idProduct = null,
        int? idWarehouse = null)
    {
        var query = _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Include(i => i.PhysicalInventory)
            .AsQueryable();

        if (idInventory.HasValue)
            query = query.Where(i => i.IdInventory == idInventory.Value);

        if (minStock.HasValue)
            query = query.Where(i => i.Stock >= minStock.Value);

        if (maxStock.HasValue)
            query = query.Where(i => i.Stock <= maxStock.Value);

        if (minStockMin.HasValue)
            query = query.Where(i => i.StockMin >= minStockMin.Value);

        if (maxStockMin.HasValue)
            query = query.Where(i => i.StockMin <= maxStockMin.Value);

        if (minStockMax.HasValue)
            query = query.Where(i => i.StockMax >= minStockMax.Value);

        if (maxStockMax.HasValue)
            query = query.Where(i => i.StockMax <= maxStockMax.Value);

        if (locationDetail.HasValue)
            query = query.Where(i => i.LocationDetail == locationDetail.Value);

        if (idPhysicalInventory.HasValue)
            query = query.Where(i => i.IdPhysicalInventory == idPhysicalInventory.Value);

        if (idProduct.HasValue)
            query = query.Where(i => i.IdProduct == idProduct.Value);

        if (idWarehouse.HasValue)
            query = query.Where(i => i.IdWarehouse == idWarehouse.Value);

        return await query.ToListAsync();
    }

    public async Task<bool> ExistsAsync(int idInventory)
    {
        return await _context.Inventories.AnyAsync(i => i.IdInventory == idInventory);
    }

    // Update
    public async Task<InventoryEntity> UpdateAsync(InventoryEntity inventory)
    {
        _context.Inventories.Update(inventory);
        await _context.SaveChangesAsync();
        return inventory;
    }

    public async Task<bool> UpdateStockAsync(int idInventory, double newStock)
    {
        var inventory = await _context.Inventories.FindAsync(idInventory);
        if (inventory == null)
            return false;

        inventory.Stock = newStock;
        await _context.SaveChangesAsync();
        return true;
    }

    // Delete
    public async Task<bool> DeleteAsync(int idInventory)
    {
        var inventory = await _context.Inventories.FindAsync(idInventory);
        if (inventory == null)
            return false;

        _context.Inventories.Remove(inventory);
        await _context.SaveChangesAsync();
        return true;
    }
}
