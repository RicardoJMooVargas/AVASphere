using AVASphere.ApplicationCore.Projects.Entities.Catalogs;
using AVASphere.ApplicationCore.Projects.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace AVASphere.Infrastructure.Projects.Repository;

public class ProjectCategoryRepository : IProjectCategoryRepository
{
    private readonly MasterDbContext _context;

    public ProjectCategoryRepository(MasterDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectCategory> CreateAsync(ProjectCategory projectCategory)
    {
        _context.ProjectCategory.Add(projectCategory);
        await _context.SaveChangesAsync();
        return projectCategory;
    }

    public async Task<ProjectCategory?> GetByIdAsync(int id)
    {
        return await _context.ProjectCategory
            .FirstOrDefaultAsync(pc => pc.IdProjectCategory == id);
    }
    
    public async Task<ProjectCategory?> GetByNameAsync(string name)
    {
        return await _context.ProjectCategory
            .FirstOrDefaultAsync(pc => pc.Name == name || pc.NormalizedName == name);
    }
   
    public async Task<IEnumerable<ProjectCategory>> GetAllAsync()
    {
        return await _context.ProjectCategory
            .ToListAsync();
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.ProjectCategory.AnyAsync(pc => pc.IdProjectCategory == id);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.ProjectCategory.AnyAsync(pc => pc.Name == name || pc.NormalizedName == name);
    }

    public async Task<ProjectCategory> UpdateAsync(ProjectCategory projectCategory)
    {
        _context.ProjectCategory.Update(projectCategory);
        await _context.SaveChangesAsync();
        return projectCategory;
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var projectCategory = await _context.ProjectCategory.FindAsync(id);
        if (projectCategory == null) return false;

        _context.ProjectCategory.Remove(projectCategory);
        await _context.SaveChangesAsync();
        return true;
    }
}