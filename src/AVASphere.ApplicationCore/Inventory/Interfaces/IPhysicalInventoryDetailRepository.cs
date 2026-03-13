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
    Task<PhysicalInventoryDetail?> GetByPhysicalInventoryAndProductAsync(int idPhysicalInventory, int idProduct);
    Task<IEnumerable<PhysicalInventoryDetail>> GetByProductIdAsync(int idProduct);
    Task<IEnumerable<PhysicalInventoryDetail>> GetDiscrepanciesAsync(int idPhysicalInventory);
    Task<IEnumerable<PhysicalInventoryDetail>> GetFilteredAsync(
        int? idPhysicalInventoryDetail = null,
        double? minSystemQuantity = null,
        double? maxSystemQuantity = null,
        double? minPhysicalQuantity = null,
        double? maxPhysicalQuantity = null,
        double? minDifference = null,
        double? maxDifference = null,
        int? idPhysicalInventory = null,
        int? idProduct = null,
        int? idLocationDetails = null,
        bool? hasDifferences = null);
    Task<bool> ExistsAsync(int idPhysicalInventoryDetail);
    
    // Update
    Task<PhysicalInventoryDetail> UpdateAsync(PhysicalInventoryDetail detail);
    
    // Delete
    Task<bool> DeleteAsync(int idPhysicalInventoryDetail);
    Task<bool> DeleteByPhysicalInventoryIdAsync(int idPhysicalInventory);
}
