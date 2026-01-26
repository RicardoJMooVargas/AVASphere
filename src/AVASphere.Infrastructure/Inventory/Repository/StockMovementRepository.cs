using AVASphere.ApplicationCore.Inventory.Entities.General;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Inventory.Repository;

public class StockMovementRepository : IStockMovementRepository
{
    private readonly MasterDbContext _context;

    public StockMovementRepository(MasterDbContext context)
    {
        _context = context;
    }

    // Create
    public async Task<StockMovement> CreateAsync(StockMovement stockMovement)
    {
        if (stockMovement is null)
            throw new ArgumentNullException(nameof(stockMovement));

        _context.Set<StockMovement>().Add(stockMovement);
        await _context.SaveChangesAsync();
        return stockMovement;
    }

    public async Task<IEnumerable<StockMovement>> CreateBatchAsync(IEnumerable<StockMovement> stockMovements)
    {
        if (stockMovements is null)
            throw new ArgumentNullException(nameof(stockMovements));
            
        var movementsList = stockMovements.ToList();
        if (!movementsList.Any())
            throw new ArgumentException("Stock movements collection cannot be empty.", nameof(stockMovements));

        await _context.Set<StockMovement>().AddRangeAsync(movementsList);
        await _context.SaveChangesAsync();
        return movementsList;
    }

    // Read
    public async Task<StockMovement?> GetByIdAsync(int idStockMovement)
    {
        return await _context.Set<StockMovement>()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .AsNoTracking()
            .FirstOrDefaultAsync(sm => sm.IdStockMovement == idStockMovement);
    }

    public async Task<IEnumerable<StockMovement>> GetAllAsync()
    {
        return await _context.Set<StockMovement>()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .AsNoTracking()
            .OrderByDescending(sm => sm.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockMovement>> GetByProductIdAsync(int idProduct)
    {
        return await _context.Set<StockMovement>()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .AsNoTracking()
            .Where(sm => sm.IdProduct == idProduct)
            .OrderByDescending(sm => sm.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockMovement>> GetByWarehouseIdAsync(int idWarehouse)
    {
        return await _context.Set<StockMovement>()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .AsNoTracking()
            .Where(sm => sm.IdWarehouse == idWarehouse)
            .OrderByDescending(sm => sm.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockMovement>> GetByMovementTypeAsync(int movementType)
    {
        return await _context.Set<StockMovement>()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .AsNoTracking()
            .Where(sm => sm.MovementType == movementType)
            .OrderByDescending(sm => sm.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<StockMovement>()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .AsNoTracking()
            .Where(sm => sm.CreatedDate >= startDate && sm.CreatedDate <= endDate)
            .OrderByDescending(sm => sm.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockMovement>> GetByProductAndWarehouseAsync(int idProduct, int idWarehouse)
    {
        return await _context.Set<StockMovement>()
            .Include(sm => sm.Product)
            .Include(sm => sm.Warehouse)
            .AsNoTracking()
            .Where(sm => sm.IdProduct == idProduct && sm.IdWarehouse == idWarehouse)
            .OrderByDescending(sm => sm.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int idStockMovement)
    {
        return await _context.Set<StockMovement>()
            .AnyAsync(sm => sm.IdStockMovement == idStockMovement);
    }

    // Update
    public async Task<StockMovement> UpdateAsync(StockMovement stockMovement)
    {
        if (stockMovement is null)
            throw new ArgumentNullException(nameof(stockMovement));

        var existingMovement = await _context.Set<StockMovement>()
            .FirstOrDefaultAsync(sm => sm.IdStockMovement == stockMovement.IdStockMovement);

        if (existingMovement is null)
            throw new KeyNotFoundException($"StockMovement with ID {stockMovement.IdStockMovement} not found.");

        _context.Entry(existingMovement).CurrentValues.SetValues(stockMovement);
        await _context.SaveChangesAsync();

        return existingMovement;
    }

    // Delete
    public async Task<bool> DeleteAsync(int idStockMovement)
    {
        var stockMovement = await _context.Set<StockMovement>()
            .FirstOrDefaultAsync(sm => sm.IdStockMovement == idStockMovement);

        if (stockMovement is null)
            return false;

        _context.Set<StockMovement>().Remove(stockMovement);
        await _context.SaveChangesAsync();
        return true;
    }
}
