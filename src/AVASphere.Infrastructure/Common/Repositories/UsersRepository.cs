using Microsoft.EntityFrameworkCore;
using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.Infrastructure;

namespace AVASphere.Infrastructure.Common.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly MasterDbContext _context;

    public UsersRepository(MasterDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<Users> SelectUserAsync(Users user) 
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var query = _context.Users.AsQueryable();

        // Aplicar filtros dinámicos basados en los campos no nulos/no vacíos
        if (user.IdUsers > 0)
            query = query.Where(u => u.IdUsers == user.IdUsers);

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

        if (user.IdRols > 0)
            query = query.Where(u => u.IdRols == user.IdRols);

        // Incluir la relación con Rols
        query = query.Include(u => u.Rols);

        return await query.FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public async Task<Users> SelectByIdAsync(int idUsers)
    {
        if (idUsers <= 0)
            throw new ArgumentException("El ID de usuario debe ser mayor a 0", nameof(idUsers));

        return await _context.Users
            .Include(u => u.Rols)
            .FirstOrDefaultAsync(u => u.IdUsers == idUsers);
    }

    /// <inheritdoc/>
    public async Task CreateUsersAsync(Users user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // Validar que el UserName no esté vacío
        if (string.IsNullOrWhiteSpace(user.UserName))
            throw new ArgumentException("El nombre de usuario no puede estar vacío", nameof(user.UserName));

        // Validar que el Rol existe
        var rolExists = await _context.Rols.AnyAsync(r => r.IdRols == user.IdRols);
        if (!rolExists)
            throw new ArgumentException($"El rol con ID {user.IdRols} no existe");

        // Establecer valores por defecto si no se proporcionan
        if (string.IsNullOrEmpty(user.Status))
            user.Status = "Active";

        if (string.IsNullOrEmpty(user.CreateAt))
            user.CreateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateUsersAsync(Users user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (user.IdUsers <= 0)
            throw new ArgumentException("El ID de usuario debe ser mayor a 0", nameof(user.IdUsers));

        // Validar que el usuario existe
        var existingUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IdUsers == user.IdUsers);

        if (existingUser == null)
            throw new ArgumentException($"El usuario con ID {user.IdUsers} no existe");

        // Validar que el Rol existe si se está actualizando
        if (user.IdRols > 0 && user.IdRols != existingUser.IdRols)
        {
            var rolExists = await _context.Rols.AnyAsync(r => r.IdRols == user.IdRols);
            if (!rolExists)
                throw new ArgumentException($"El rol con ID {user.IdRols} no existe");
        }

        // Actualizar el usuario
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}