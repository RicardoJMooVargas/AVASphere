using AVASphere.ApplicationCore.Inventory.Entities.General;

namespace AVASphere.ApplicationCore.Inventory.Interfaces;

public interface IPhysicalInventoryRepository
{
    // Create
    Task<PhysicalInventory> CreateAsync(PhysicalInventory physicalInventory);
    
    // Read
    Task<PhysicalInventory?> GetByIdAsync(int idPhysicalInventory);
    Task<PhysicalInventory?> GetByIdWithDetailsAsync(int idPhysicalInventory);
    Task<IEnumerable<PhysicalInventory>> GetAllAsync();
    Task<IEnumerable<PhysicalInventory>> GetByWarehouseIdAsync(int idWarehouse);
    Task<IEnumerable<PhysicalInventory>> GetByStatusAsync(string status);
    Task<IEnumerable<PhysicalInventory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> ExistsAsync(int idPhysicalInventory);
    
    // Update
    Task<PhysicalInventory> UpdateAsync(PhysicalInventory physicalInventory);
    Task<bool> UpdateStatusAsync(int idPhysicalInventory, string status);
    
    // Delete
    Task<bool> DeleteAsync(int idPhysicalInventory);
}
