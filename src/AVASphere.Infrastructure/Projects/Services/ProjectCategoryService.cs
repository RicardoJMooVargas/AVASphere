using AVASphere.ApplicationCore.Projects.DTOs;
using AVASphere.ApplicationCore.Projects.Entities;
using AVASphere.ApplicationCore.Projects.Entities.Catalogs;
using AVASphere.ApplicationCore.Projects.Interfaces;
using Microsoft.Extensions.Logging;

namespace AVASphere.Infrastructure.Projects.Services;

public class ProjectCategoryService : IProjectCategoryService
{
    private readonly IProjectCategoryRepository _projectCategoryRepository;
    private readonly ILogger<ProjectCategoryService> _logger;

    public ProjectCategoryService(IProjectCategoryRepository projectCategoryRepository, ILogger<ProjectCategoryService> logger)
    {
        _projectCategoryRepository = projectCategoryRepository;
        _logger = logger;
    }
    
    public async Task<ProjectCategoryResponseDto> CreateAsync(ProjectCategoryRequestDto projectCategoryRequest)
        {
            try
            {
                // Validar si ya existe una categoría con el mismo nombre
                var existingProjectCategory = await _projectCategoryRepository.GetByNameAsync(projectCategoryRequest.Name);
                if (existingProjectCategory != null)
                {
                    throw new InvalidOperationException($"Ya existe una categoría con el nombre: {projectCategoryRequest.Name}");
                }
    
                var projectCategory = new ProjectCategory
                {
                    Name = projectCategoryRequest.Name,
                    NormalizedName = projectCategoryRequest.NormalizedName ?? projectCategoryRequest.Name.ToUpper()
                };
    
                var createdProjectCategory = await _projectCategoryRepository.CreateAsync(projectCategory);
                
                return new ProjectCategoryResponseDto
                {
                    IdProjectCategory = createdProjectCategory.IdProjectCategory,
                    Name = createdProjectCategory.Name,
                    NormalizedName = createdProjectCategory.NormalizedName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la categoría: {ProjectCategoryName}", projectCategoryRequest.Name);
                throw;
            }
        }
    
    public async Task<ProjectCategoryResponseDto?> GetByIdAsync(int id)
    {
        try
        {
            var projectCategory = await _projectCategoryRepository.GetByIdAsync(id);
            if (projectCategory == null) return null;

            return new ProjectCategoryResponseDto
            {
                IdProjectCategory = projectCategory.IdProjectCategory,
                Name = projectCategory.Name,
                NormalizedName = projectCategory.NormalizedName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la categoría por ID: {ProjectCategoryId}", id);
            throw;
        }
    }
    
    public async Task<ProjectCategoryResponseDto?> GetByNameAsync(string name)
    {
        try
        {
            var projectCategory = await _projectCategoryRepository.GetByNameAsync(name);
            if (projectCategory == null) return null;

            return new ProjectCategoryResponseDto
            {
                IdProjectCategory = projectCategory.IdProjectCategory,
                Name = projectCategory.Name,
                NormalizedName = projectCategory.NormalizedName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la categoría por nombre: {ProjectCategoryName}", name);
            throw;
        }
    }
    
    public async Task<IEnumerable<ProjectCategoryResponseDto>> GetAllAsync()
    {
        try
        {
            var projectCategories = await _projectCategoryRepository.GetAllAsync();
            
            return projectCategories.Select(projectCategory => new ProjectCategoryResponseDto
            {
                IdProjectCategory = projectCategory.IdProjectCategory,
                Name = projectCategory.Name,
                NormalizedName = projectCategory.NormalizedName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todas las categorías");
            throw;
        }
    }
    
    public async Task<ProjectCategoryResponseDto> UpdateAsync(int id, ProjectCategoryRequestDto projectCategoryRequest)
    {
        try
        {
            var existingProjectCategory = await _projectCategoryRepository.GetByIdAsync(id);
            if (existingProjectCategory == null)
            {
                throw new KeyNotFoundException($"Categoría con ID {id} no encontrada");
            }

            // Validar si el nuevo nombre ya existe en otra categoría
            var projectCategoryWithSameName = await _projectCategoryRepository.GetByNameAsync(projectCategoryRequest.Name);
            if (projectCategoryWithSameName != null && projectCategoryWithSameName.IdProjectCategory != id)
            {
                throw new InvalidOperationException($"Ya existe otra categoría con el nombre: {projectCategoryRequest.Name}");
            }

            existingProjectCategory.Name = projectCategoryRequest.Name;
            existingProjectCategory.NormalizedName = projectCategoryRequest.NormalizedName ?? projectCategoryRequest.Name.ToUpper();

            var updatedProjectCategory = await _projectCategoryRepository.UpdateAsync(existingProjectCategory);
            
            return new ProjectCategoryResponseDto
            {
                IdProjectCategory = updatedProjectCategory.IdProjectCategory,
                Name = updatedProjectCategory.Name,
                NormalizedName = updatedProjectCategory.NormalizedName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la categoría: {ProjectCategoryId}", id);
            throw;
        }
    }
}