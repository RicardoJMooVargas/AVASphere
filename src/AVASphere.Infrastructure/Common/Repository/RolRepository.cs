using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Common.Repository;
public class RolRepository : IRolRepository
{
    private readonly MasterDbContext _context;

    public RolRepository(MasterDbContext context)
    {
        _context = context;
    }

    public async Task<Rol> CreateAsync(Rol rol)
    {
        _context.Rols.Add(rol); // Usar Rols (plural)
        await _context.SaveChangesAsync();
        return rol;
    }

    public async Task<Rol?> GetByIdAsync(int id)
    {
        return await _context.Rols
            .Include(r => r.Area)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.IdRol == id);
    }

    public async Task<Rol?> GetByNameAsync(string name)
    {
        return await _context.Rols
            .Include(r => r.Area)
            .FirstOrDefaultAsync(r => r.Name == name || r.NormalizedName == name);
    }

    public async Task<IEnumerable<Rol>> GetAllAsync()
    {
        return await _context.Rols
            .Include(r => r.Area)
            .ToListAsync();
    }

    public async Task<IEnumerable<Rol>> GetByAreaAsync(int areaId)
    {
        return await _context.Rols
            .Include(r => r.Area)
            .Where(r => r.IdArea == areaId)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Rols.AnyAsync(r => r.IdRol == id);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Rols.AnyAsync(r => r.Name == name || r.NormalizedName == name);
    }

    public async Task<Rol> UpdateAsync(Rol rol)
    {
        _context.Rols.Update(rol);
        await _context.SaveChangesAsync();
        return rol;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var rol = await _context.Rols.FindAsync(id);
        if (rol == null) return false;

        _context.Rols.Remove(rol);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdatePermissionsAsync(int rolId, List<Permission> permissions)
    {
        var rol = await _context.Rols.FindAsync(rolId);
        if (rol == null) return false;

        rol.Permissions = permissions;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateModulesAsync(int rolId, List<ApplicationCore.Common.Enums.SystemModule> moduleEnums)
    {
        var rol = await _context.Rols.FindAsync(rolId);
        if (rol == null) return false;

        // Conversión directa aquí
        rol.Modules = moduleEnums.Select(me => new Module
        {
            Index = (int)me,
            Name = me.ToString()
        }).ToList();

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Permission>?> GetPermissionsAsync(int rolId)
    {
        var rol = await _context.Rols
            .Where(r => r.IdRol == rolId)
            .Select(r => r.Permissions)
            .FirstOrDefaultAsync();
        
        return rol;
    }

    public async Task<List<Module>?> GetModulesAsync(int rolId)
    {
        var rol = await _context.Rols
            .Where(r => r.IdRol == rolId)
            .Select(r => r.Modules)
            .FirstOrDefaultAsync();
        
        return rol;
    }
}