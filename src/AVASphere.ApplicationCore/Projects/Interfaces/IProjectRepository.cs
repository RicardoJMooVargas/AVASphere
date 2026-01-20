using AVASphere.ApplicationCore.Projects.Entities.General;

namespace AVASphere.ApplicationCore.Projects.Interfaces;

public interface IProjectRepository
{
    // Read Operations
    Task<IEnumerable<Project>> GetAllProjectsAsync();
    Task<Project?> GetProjectByIdAsync(int idProject);
    Task<IEnumerable<Project>> GetProjectsByCustomerIdAsync(int idCustomer);
    Task<IEnumerable<Project>> GetProjectsByHitoAsync(int hito);
    Task<IEnumerable<Project>> GetProjectsByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    // Create
    Task<Project> CreateProjectAsync(Project project);
    
    // Update
    Task<Project> UpdateProjectAsync(Project project);
    
    // Delete
    Task<bool> DeleteProjectAsync(int idProject);
    
    // Utility Methods
    Task<bool> ProjectExistsAsync(int idProject);
    Task<int> GetTotalProjectsCountAsync();
    Task<int> GetProjectsCountByCustomerAsync(int idCustomer);
    Task<Project?> GetProjectWithRelationsAsync(int idProject);
    
    // Main GET endpoint - Get customers with their projects and filters
    Task<IEnumerable<Project>> GetProjectsWithFiltersAsync(
        int? idCustomer = null, 
        int? currentHito = null, 
        List<int>? categoryIds = null);
}