using AVASphere.ApplicationCore.Inventory.Entities.General;

namespace AVASphere.ApplicationCore.Inventory.Interfaces;

public interface IStorageStructureRepository
{
    // Create
    Task<StorageStructure> CreateAsync(StorageStructure storageStructure);
    
    // Read
    Task<StorageStructure?> GetByIdAsync(int idStorageStructure);
    Task<StorageStructure?> GetByCodeAsync(string codeRack);
    Task<IEnumerable<StorageStructure>> GetAllAsync();
    Task<IEnumerable<StorageStructure>> GetByWarehouseAsync(int idWarehouse);
    Task<IEnumerable<StorageStructure>> GetByWarehouseAndAreaAsync(int idWarehouse, int idArea);
    Task<IEnumerable<StorageStructure>> GetFilteredAsync(
        int? idStorageStructure = null,
        string? codeRack = null,
        bool? oneSection = null,
        bool? hasLevel = null,
        bool? hasSubLevel = null,
        int? idWarehouse = null,
        int? idArea = null);
    Task<bool> ExistsAsync(int idStorageStructure);
    Task<bool> ExistsByCodeAsync(string codeRack);
    
    // Update
    Task<StorageStructure> UpdateAsync(StorageStructure storageStructure);
    
    // Delete
    Task<bool> DeleteAsync(int idStorageStructure);
}
