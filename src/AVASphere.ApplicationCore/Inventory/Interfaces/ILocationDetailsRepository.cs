using AVASphere.ApplicationCore.Inventory.Entities.General;

namespace AVASphere.ApplicationCore.Inventory.Interfaces;

public interface ILocationDetailsRepository
{
    // Create
    Task<LocationDetails> CreateAsync(LocationDetails locationDetails);
    
    // Read
    Task<LocationDetails?> GetByIdAsync(int idLocationDetails);
    Task<IEnumerable<LocationDetails>> GetAllAsync();
    Task<IEnumerable<LocationDetails>> GetByAreaIdAsync(int idArea);
    Task<IEnumerable<LocationDetails>> GetByStorageStructureIdAsync(int idStorageStructure);
    Task<LocationDetails?> GetByLocationParametersAsync(int idArea, int idStorageStructure, string section, int verticalLevel);
    Task<bool> ExistsAsync(int idLocationDetails);
    
    // Update
    Task<LocationDetails> UpdateAsync(LocationDetails locationDetails);
    
    // Delete
    Task<bool> DeleteAsync(int idLocationDetails);
}
