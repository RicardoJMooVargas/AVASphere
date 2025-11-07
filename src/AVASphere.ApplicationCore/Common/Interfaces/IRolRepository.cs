using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Entities.General;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IRolRepository
{
    // Create
    Task<Rol> CreateAsync(Rol rol);
    
    // Read
    Task<Rol?> GetByIdAsync(int id);
    Task<Rol?> GetByNameAsync(string name);
    Task<IEnumerable<Rol>> GetAllAsync();
    Task<IEnumerable<Rol>> GetByAreaAsync(int areaId);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNameAsync(string name);
    
    // Update
    Task<Rol> UpdateAsync(Rol rol);
    
    // Delete
    Task<bool> DeleteAsync(int id);
    // AUN NO IMPLEMENTAR en SERVICIOS
    // Métodos específicos para permisos y módulos
    Task<bool> UpdatePermissionsAsync(int rolId, List<Permission> permissions);
    Task<bool> UpdateModulesAsync(int rolId, List<ApplicationCore.Common.Enums.SystemModule> moduleEnums);
    Task<List<Permission>?> GetPermissionsAsync(int rolId);
    Task<List<Module>?> GetModulesAsync(int rolId);
}