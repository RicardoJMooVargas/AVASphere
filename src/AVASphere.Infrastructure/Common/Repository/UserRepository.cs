using Microsoft.EntityFrameworkCore;
using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.Infrastructure;

namespace AVASphere.Infrastructure.Common.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MasterDbContext _context;

    public UserRepository(MasterDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<User> SelectUserAsync(User user) 
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var query = _context.Users.AsQueryable();

        // Aplicar filtros dinámicos basados en los campos no nulos/no vacíos
        if (user.IdUser > 0)
            query = query.Where(u => u.IdUser == user.IdUser);

        if (!string.IsNullOrEmpty(user.UserName))
            query = query.Where(u => u.UserName.ToLower() == user.UserName.ToLower());

        if (!string.IsNullOrEmpty(user.Name))
            query = query.Where(u => u.Name != null && u.Name.ToLower().Contains(user.Name.ToLower()));

        if (!string.IsNullOrEmpty(user.LastName))
            query = query.Where(u => u.LastName != null && u.LastName.ToLower().Contains(user.LastName.ToLower()));

        if (!string.IsNullOrEmpty(user.Status))
            query = query.Where(u => u.Status == user.Status);

        if (!string.IsNullOrEmpty(user.Verified))
            query = query.Where(u => u.Verified == user.Verified);

        if (user.IdRol > 0)
            query = query.Where(u => u.IdRol == user.IdRol);

        // Incluir la relación con Rol
        query = query.Include(u => u.Rol);

        return await query.FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public async Task<User> SelectByIdAsync(int idUsers)
    {
        if (idUsers <= 0)
            throw new ArgumentException("El ID de usuario debe ser mayor a 0", nameof(idUsers));

        return await _context.Users
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.IdUser == idUsers);
    }

    /// <inheritdoc/>
    public async Task CreateUsersAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // Validar que el UserName no esté vacío
        if (string.IsNullOrWhiteSpace(user.UserName))
            throw new ArgumentException("El nombre de usuario no puede estar vacío", nameof(user.UserName));

        // Validar que el Rol existe
        var rolExists = await _context.Rols.AnyAsync(r => r.IdRol == user.IdRol);
        if (!rolExists)
            throw new ArgumentException($"El rol con ID {user.IdRol} no existe");

        // Establecer valores por defecto si no se proporcionan
        if (string.IsNullOrEmpty(user.Status))
            user.Status = "Active";

        if (string.IsNullOrEmpty(user.CreateAt))
            user.CreateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateUsersAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (user.IdUser <= 0)
            throw new ArgumentException("El ID de usuario debe ser mayor a 0", nameof(user.IdUser));

        // Validar que el usuario existe
        var existingUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IdUser == user.IdUser);

        if (existingUser == null)
            throw new ArgumentException($"El usuario con ID {user.IdUser} no existe");

        // Validar que el Rol existe si se está actualizando
        if (user.IdRol > 0 && user.IdRol != existingUser.IdRol)
        {
            var rolExists = await _context.Rols.AnyAsync(r => r.IdRol == user.IdRol);
            if (!rolExists)
                throw new ArgumentException($"El rol con ID {user.IdRol} no existe");
        }

        // Actualizar el usuario
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}