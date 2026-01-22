using InventoryEntity = AVASphere.ApplicationCore.Inventory.Entities.General.Inventory;

namespace AVASphere.ApplicationCore.Inventory.Interfaces;

public interface IInventoryRepository
{
    // Create
    Task<InventoryEntity> CreateAsync(InventoryEntity inventory);
    
    // Read
    Task<InventoryEntity?> GetByIdAsync(int idInventory);
    Task<IEnumerable<InventoryEntity>> GetAllAsync();
    Task<IEnumerable<InventoryEntity>> GetByWarehouseIdAsync(int idWarehouse);
    Task<IEnumerable<InventoryEntity>> GetByProductIdAsync(int idProduct);
    Task<InventoryEntity?> GetByWarehouseAndProductAsync(int idWarehouse, int idProduct);
    Task<IEnumerable<InventoryEntity>> GetLowStockItemsAsync();
    Task<bool> ExistsAsync(int idInventory);
    
    // Update
    Task<InventoryEntity> UpdateAsync(InventoryEntity inventory);
    Task<bool> UpdateStockAsync(int idInventory, double newStock);
    
    // Delete
    Task<bool> DeleteAsync(int idInventory);
}
