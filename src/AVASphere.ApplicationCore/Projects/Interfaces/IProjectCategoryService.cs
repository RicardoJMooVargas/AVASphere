using AVASphere.ApplicationCore.Projects.DTOs;

namespace AVASphere.ApplicationCore.Projects.Interfaces;

public interface IProjectCategoryService
{
    // Create
    Task<ProjectCategoryResponseDto> CreateAsync(ProjectCategoryRequestDto projectCategoryRequest);
    
    // Read
    Task<ProjectCategoryResponseDto?> GetByIdAsync(int id);
    Task<ProjectCategoryResponseDto?> GetByNameAsync(string name);
    Task<IEnumerable<ProjectCategoryResponseDto>> GetAllAsync();
    
    // Update
    Task<ProjectCategoryResponseDto> UpdateAsync(int id, ProjectCategoryRequestDto projectCategoryRequest);
    
    // Delete
    //Task<bool> DeleteAsync(int id);
}