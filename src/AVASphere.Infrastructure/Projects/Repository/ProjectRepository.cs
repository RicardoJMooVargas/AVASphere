using AVASphere.ApplicationCore.Projects.Entities.General;
using AVASphere.ApplicationCore.Projects.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Projects.Repository;

public class ProjectRepository : IProjectRepository
{
    private readonly MasterDbContext _context;

    public ProjectRepository(MasterDbContext context)
    {
        _context = context;
    }

    // Read Operations
    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
    {
        return await _context.Set<Project>()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Project?> GetProjectByIdAsync(int idProject)
    {
        return await _context.Set<Project>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdProject == idProject);
    }

    public async Task<IEnumerable<Project>> GetProjectsByCustomerIdAsync(int idCustomer)
    {
        return await _context.Set<Project>()
            .Where(p => p.IdCustomer == idCustomer)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetProjectsByHitoAsync(int hito)
    {
        return await _context.Set<Project>()
            .Where(p => (int)p.CurrentHito == hito)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetProjectsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<Project>()
            .Where(p => p.AppointmentJson != null &&
                        p.AppointmentJson.Datetime >= startDate &&
                        p.AppointmentJson.Datetime <= endDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Project?> GetProjectWithRelationsAsync(int idProject)
    {
        return await _context.Set<Project>()
            .Include(p => p.Customer)
            .Include(p => p.ConfigSys)
            .Include(p => p.ProjectQuote)
            .Include(p => p.ListOfCategories)
                .ThenInclude(lc => lc.ProjectCategory)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdProject == idProject);
    }

    // Create
    public async Task<Project> CreateProjectAsync(Project project)
    {
        if (project is null)
            throw new ArgumentNullException(nameof(project));

        _context.Set<Project>().Add(project);
        await _context.SaveChangesAsync();
        return project;
    }

    // Update
    public async Task<Project> UpdateProjectAsync(Project project)
    {
        if (project is null)
            throw new ArgumentNullException(nameof(project));

        var tracked = await _context.Set<Project>().FindAsync(project.IdProject);
        if (tracked == null)
        {
            _context.Set<Project>().Update(project);
        }
        else
        {
            _context.Entry(tracked).CurrentValues.SetValues(project);
            tracked.AppointmentJson = project.AppointmentJson;
            tracked.VisitsJson = project.VisitsJson;
        }

        await _context.SaveChangesAsync();
        return project;
    }

    // Delete
    public async Task<bool> DeleteProjectAsync(int idProject)
    {
        var entity = await _context.Set<Project>().FindAsync(idProject);
        if (entity == null)
            return false;

        _context.Set<Project>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // Utility Methods
    public async Task<bool> ProjectExistsAsync(int idProject)
    {
        return await _context.Set<Project>()
            .AnyAsync(p => p.IdProject == idProject);
    }

    public async Task<int> GetTotalProjectsCountAsync()
    {
        return await _context.Set<Project>().CountAsync();
    }

    public async Task<int> GetProjectsCountByCustomerAsync(int idCustomer)
    {
        return await _context.Set<Project>()
            .CountAsync(p => p.IdCustomer == idCustomer);
    }

    // Main GET endpoint - Get projects with filters and full relations
    public async Task<IEnumerable<Project>> GetProjectsWithFiltersAsync(
        int? idCustomer = null, 
        int? currentHito = null, 
        List<int>? categoryIds = null)
    {
        var query = _context.Set<Project>()
            .Include(p => p.Customer)
            .Include(p => p.ProjectQuote)
            .Include(p => p.ListOfCategories)
                .ThenInclude(lc => lc.ProjectCategory)
            .AsNoTracking()
            .AsQueryable();

        // Apply filters dynamically
        if (idCustomer.HasValue)
        {
            query = query.Where(p => p.IdCustomer == idCustomer.Value);
        }

        if (currentHito.HasValue)
        {
            query = query.Where(p => (int)p.CurrentHito == currentHito.Value);
        }

        if (categoryIds != null && categoryIds.Any())
        {
            query = query.Where(p => p.ListOfCategories.Any(lc => categoryIds.Contains(lc.IdProjectCategory)));
        }

        return await query.ToListAsync();
    }
}