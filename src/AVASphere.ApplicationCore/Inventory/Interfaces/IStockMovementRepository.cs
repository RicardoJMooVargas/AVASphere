using AVASphere.ApplicationCore.Inventory.Entities.General;

namespace AVASphere.ApplicationCore.Inventory.Interfaces;

public interface IStockMovementRepository
{
    // Create
    Task<StockMovement> CreateAsync(StockMovement stockMovement);
    Task<IEnumerable<StockMovement>> CreateBatchAsync(IEnumerable<StockMovement> stockMovements);
    
    // Read
    Task<StockMovement?> GetByIdAsync(int idStockMovement);
    Task<IEnumerable<StockMovement>> GetAllAsync();
    Task<IEnumerable<StockMovement>> GetByProductIdAsync(int idProduct);
    Task<IEnumerable<StockMovement>> GetByWarehouseIdAsync(int idWarehouse);
    Task<IEnumerable<StockMovement>> GetByMovementTypeAsync(int movementType);
    Task<IEnumerable<StockMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<StockMovement>> GetByProductAndWarehouseAsync(int idProduct, int idWarehouse);
    Task<bool> ExistsAsync(int idStockMovement);
    
    // Update
    Task<StockMovement> UpdateAsync(StockMovement stockMovement);
    
    // Delete
    Task<bool> DeleteAsync(int idStockMovement);
}


