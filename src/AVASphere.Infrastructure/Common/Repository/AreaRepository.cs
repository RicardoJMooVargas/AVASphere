using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Common.Repository;


public class AreaRepository : IAreaRepository
{
    private readonly MasterDbContext _context;

    public AreaRepository(MasterDbContext context)
    {
        _context = context;
    }

    public async Task<Area> CreateAsync(Area area)
    {
        _context.Areas.Add(area);
        await _context.SaveChangesAsync();
        return area;
    }

    public async Task<Area?> GetByIdAsync(int id)
    {
        return await _context.Areas
            .Include(a => a.Rol)
            .FirstOrDefaultAsync(a => a.IdArea == id);
    }

    public async Task<Area?> GetByNameAsync(string name)
    {
        return await _context.Areas
            .FirstOrDefaultAsync(a => a.Name == name || a.NormalizedName == name);
    }

    public async Task<IEnumerable<Area>> GetAllAsync()
    {
        return await _context.Areas
            .Include(a => a.Rol)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Areas.AnyAsync(a => a.IdArea == id);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Areas.AnyAsync(a => a.Name == name || a.NormalizedName == name);
    }

    public async Task<Area> UpdateAsync(Area area)
    {
        _context.Areas.Update(area);
        await _context.SaveChangesAsync();
        return area;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var area = await _context.Areas.FindAsync(id);
        if (area == null) return false;

        _context.Areas.Remove(area);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Rol>> GetRolesByAreaAsync(int areaId)
    {
        return await _context.Rols
            .Where(r => r.IdArea == areaId)
            .ToListAsync();
    }
}