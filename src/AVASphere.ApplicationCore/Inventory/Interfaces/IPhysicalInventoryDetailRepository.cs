using AVASphere.ApplicationCore.Inventory.Entities.General;

namespace AVASphere.ApplicationCore.Inventory.Interfaces;

public interface IPhysicalInventoryDetailRepository
{
    // Create
    Task<PhysicalInventoryDetail> CreateAsync(PhysicalInventoryDetail detail);
    Task<IEnumerable<PhysicalInventoryDetail>> CreateBatchAsync(IEnumerable<PhysicalInventoryDetail> details);
    
    // Read
    Task<PhysicalInventoryDetail?> GetByIdAsync(int idPhysicalInventoryDetail);
    Task<IEnumerable<PhysicalInventoryDetail>> GetAllAsync();
    Task<IEnumerable<PhysicalInventoryDetail>> GetByPhysicalInventoryIdAsync(int idPhysicalInventory);
    Task<IEnumerable<PhysicalInventoryDetail>> GetByProductIdAsync(int idProduct);
    Task<IEnumerable<PhysicalInventoryDetail>> GetDiscrepanciesAsync(int idPhysicalInventory);
    Task<bool> ExistsAsync(int idPhysicalInventoryDetail);
    
    // Update
    Task<PhysicalInventoryDetail> UpdateAsync(PhysicalInventoryDetail detail);
    
    // Delete
    Task<bool> DeleteAsync(int idPhysicalInventoryDetail);
    Task<bool> DeleteByPhysicalInventoryIdAsync(int idPhysicalInventory);
}
