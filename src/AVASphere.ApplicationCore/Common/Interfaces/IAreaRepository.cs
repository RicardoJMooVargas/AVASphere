using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Entities.General;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IAreaRepository
{
    // Create
    Task<Area> CreateAsync(Area area);
    
    // Read
    Task<Area?> GetByIdAsync(int id);
    Task<Area?> GetByNameAsync(string name);
    Task<IEnumerable<Area>> GetAllAsync();
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNameAsync(string name);
    
    // Update
    Task<Area> UpdateAsync(Area area);
    
    // Delete
    Task<bool> DeleteAsync(int id);
    
    // Relaciones
    Task<IEnumerable<Rol>> GetRolesByAreaAsync(int areaId);
}