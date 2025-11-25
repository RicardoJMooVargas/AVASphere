using AVASphere.ApplicationCore.Common.Entities.Catalogs;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IPropertyRepository
{
    // Create
    Task<Property> CreateAsync(Property property);
    
    // Read
    Task<Property?> GetByIdAsync(int id);
    Task<Property?> GetByNameAsync(string name);
    Task<IEnumerable<Property>> GetAllAsync();
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNameAsync(string name);
    
    // Update
    Task<Property> UpdateAsync(Property property);
    
    // Delete
    Task<bool> DeleteAsync(int id);
}