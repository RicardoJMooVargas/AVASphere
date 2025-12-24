using AVASphere.ApplicationCore.Projects.Entities.General;
using AVASphere.ApplicationCore.Projects.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Projects.Repository;

public class ListOfCategoriesRepository
{
    private readonly MasterDbContext _context;
    private readonly IProjectCategoryRepository _projectCategoryRepository;

    public ListOfCategoriesRepository(MasterDbContext context, IProjectCategoryRepository projectCategoryRepository)
    {
        _context = context;
        _projectCategoryRepository = projectCategoryRepository;
    }

    /// <summary>
    /// Crea múltiples categorías para un proyecto
    /// </summary>
    public async Task<IEnumerable<ListOfCategories>> CreateCategoriesForProjectAsync(int idProject, List<int> categoryIds)
    {
        if (categoryIds == null || !categoryIds.Any())
            throw new ArgumentException("Category IDs list cannot be empty", nameof(categoryIds));

        // Validar que todas las categorías existan
        var validCategories = new List<int>();
        foreach (var categoryId in categoryIds)
        {
            var exists = await _projectCategoryRepository.ExistsAsync(categoryId);
            if (!exists)
                throw new KeyNotFoundException($"Project category with Id {categoryId} not found.");
            
            validCategories.Add(categoryId);
        }

        // Crear las relaciones
        var listOfCategories = new List<ListOfCategories>();
        foreach (var categoryId in validCategories)
        {
            var listCategory = new ListOfCategories
            {
                IdProject = idProject,
                IdProjectCategory = categoryId,
                SolutionsJson = new AVASphere.ApplicationCore.Projects.Entities.jsons.SolutionsJson()
            };
            
            _context.Set<ListOfCategories>().Add(listCategory);
            listOfCategories.Add(listCategory);
        }

        await _context.SaveChangesAsync();
        return listOfCategories;
    }

    /// <summary>
    /// Obtiene todas las categorías de un proyecto
    /// </summary>
    public async Task<IEnumerable<ListOfCategories>> GetCategoriesByProjectIdAsync(int idProject)
    {
        return await _context.Set<ListOfCategories>()
            .Include(lc => lc.ProjectCategory)
            .Where(lc => lc.IdProject == idProject)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Elimina todas las categorías de un proyecto
    /// </summary>
    public async Task<bool> DeleteCategoriesByProjectIdAsync(int idProject)
    {
        var categories = await _context.Set<ListOfCategories>()
            .Where(lc => lc.IdProject == idProject)
            .ToListAsync();

        if (!categories.Any())
            return false;

        _context.Set<ListOfCategories>().RemoveRange(categories);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Verifica si existe una categoría específica en un proyecto
    /// </summary>
    public async Task<bool> CategoryExistsInProjectAsync(int idProject, int idCategory)
    {
        return await _context.Set<ListOfCategories>()
            .AnyAsync(lc => lc.IdProject == idProject && lc.IdProjectCategory == idCategory);
    }
}

