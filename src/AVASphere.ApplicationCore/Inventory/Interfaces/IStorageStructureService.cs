using AVASphere.ApplicationCore.Inventory.DTOs;

namespace AVASphere.ApplicationCore.Inventory.Interfaces;

public interface IStorageStructureService
{
    // Create
    Task<StorageStructureResponseDto> CreateAsync(StorageStructureRequestDto storageStructureRequest);
    
    // Read
    Task<StorageStructureResponseDto?> GetByIdAsync(int id);
    Task<StorageStructureResponseDto?> GetByCodeRackAsync(string codeRack);
    Task<IEnumerable<StorageStructureResponseDto>> GetAllAsync();
    Task<IEnumerable<StorageStructureResponseDto>> GetByWarehouseIdAsync(int warehouseId);
    Task<IEnumerable<StorageStructureResponseDto>> GetByAreaIdAsync(int areaId);
    
    // Update
    Task<StorageStructureResponseDto> UpdateAsync(int id, StorageStructureRequestDto storageStructureRequest);
    
    // Delete
    Task<bool> DeleteAsync(int id);
}
