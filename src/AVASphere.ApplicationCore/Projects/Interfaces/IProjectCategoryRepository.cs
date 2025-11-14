using AVASphere.ApplicationCore.Projects.Entities.Catalogs;

namespace AVASphere.ApplicationCore.Projects.Interfaces;

public interface IProjectCategoryRepository
{
    // Create
    Task<ProjectCategory> CreateAsync(ProjectCategory projectCategory);
    
    // Read
    Task<ProjectCategory?> GetByIdAsync(int id);
    Task<ProjectCategory?> GetByNameAsync(string name);
    Task<IEnumerable<ProjectCategory>> GetAllAsync();
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNameAsync(string name);
    
    // Update
    Task<ProjectCategory> UpdateAsync(ProjectCategory projectCategory);
    
    // Delete
    Task<bool> DeleteAsync(int id);
}