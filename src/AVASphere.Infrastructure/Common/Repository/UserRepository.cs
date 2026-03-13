﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.Infrastructure;

namespace AVASphere.Infrastructure.Common.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MasterDbContext _context;
    private readonly ILogger<UserRepository> _logger; 

    public UserRepository(MasterDbContext context, ILogger<UserRepository> logger) // Modifica el constructor
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Asigna el logger
    }

    public async Task<User> SelectUserAsync(User user) 
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var query = _context.Users.AsQueryable();

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

        if (user.Verified != null)
        {
            query = query.Where(u => u.Verified == user.Verified);
        }

        if (user.IdRol > 0)
            query = query.Where(u => u.IdRol == user.IdRol);

        // ✅ NUEVO: Filtrar por IdConfigSys si se proporciona
        if (user.IdConfigSys > 0)
            query = query.Where(u => u.IdConfigSys == user.IdConfigSys);

        // Incluir relaciones
        query = query.Include(u => u.Rol)
                     .Include(u => u.ConfigSys);
    
        _logger.LogWarning(
            "FILTROS => UserName={UserName}, Status={Status}, Verified={Verified}",
            user.UserName,
            user.Status,
            user.Verified
        );

        return await query.FirstOrDefaultAsync();
    }

    public async Task<User> SelectByIdAsync(int idUsers)
    {
        if (idUsers <= 0)
            throw new ArgumentException("El ID de usuario debe ser mayor a 0", nameof(idUsers));

        return await _context.Users
            .Include(u => u.Rol)
                .ThenInclude(r => r.Area)
            .Include(u => u.ConfigSys)
            .FirstOrDefaultAsync(u => u.IdUser == idUsers);
    }

    public async Task CreateUsersAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrWhiteSpace(user.UserName))
            throw new ArgumentException("El nombre de usuario no puede estar vacío", nameof(user.UserName));

        // ✅ VALIDAR QUE ROL EXISTA
        var rolExists = await _context.Rols.AnyAsync(r => r.IdRol == user.IdRol);
        if (!rolExists)
            throw new ArgumentException($"El rol con ID {user.IdRol} no existe");

        // ✅ VALIDAR QUE CONFIG SYS EXISTA
        var configExists = await _context.ConfigSys.AnyAsync(c => c.IdConfigSys == user.IdConfigSys);
        if (!configExists)
            throw new ArgumentException($"La configuración del sistema con ID {user.IdConfigSys} no existe");

        // Establecer valores por defecto
        if (string.IsNullOrEmpty(user.Status))
            user.Status = "Active";

        if (user.CreateAt == null)
            user.CreateAt = DateOnly.FromDateTime(DateTime.Now);

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUsersAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (user.IdUser <= 0)
            throw new ArgumentException("El ID de usuario debe ser mayor a 0", nameof(user.IdUser));

        var existingUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IdUser == user.IdUser);

        if (existingUser == null)
            throw new ArgumentException($"El usuario con ID {user.IdUser} no existe");

        // ✅ VALIDAR QUE ROL EXISTA SI SE ACTUALIZA
        if (user.IdRol > 0 && user.IdRol != existingUser.IdRol)
        {
            var rolExists = await _context.Rols.AnyAsync(r => r.IdRol == user.IdRol);
            if (!rolExists)
                throw new ArgumentException($"El rol con ID {user.IdRol} no existe");
        }

        // ✅ VALIDAR QUE CONFIG SYS EXISTA SI SE ACTUALIZA
        if (user.IdConfigSys > 0 && user.IdConfigSys != existingUser.IdConfigSys)
        {
            var configExists = await _context.ConfigSys.AnyAsync(c => c.IdConfigSys == user.IdConfigSys);
            if (!configExists)
                throw new ArgumentException($"La configuración del sistema con ID {user.IdConfigSys} no existe");
        }

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Rol)
            .Include(u => u.ConfigSys)
            .ToListAsync();
    }
}