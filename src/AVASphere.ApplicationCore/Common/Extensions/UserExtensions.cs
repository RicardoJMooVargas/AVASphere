using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Entities.General;

namespace AVASphere.ApplicationCore.Common.Extensions;

public static class UserExtensions
{
    // ✅ MÉTODO ToEntity PARA UserCreateRequest
    public static User ToEntity(this UserCreateRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        return new User
        {
            UserName = request.UserName ?? throw new ArgumentNullException(nameof(request.UserName)),
            Name = request.Name,
            LastName = request.LastName,
            HashPassword = null, // Se establecerá en el servicio
            Status = "Active",
            Aux = request.Aux,
            Verified = request.Verified ?? false,
            IdRol = request.IdRols,
            IdConfigSys = request.IdConfigSys // ✅ INCLUIR IdConfigSys
        };
    }

    // ✅ MÉTODO ToEntity PARA UserUpdateRequest
    public static User ToEntity(this UserUpdateRequest request, User existingUser)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        
        if (existingUser == null)
            throw new ArgumentNullException(nameof(existingUser));

        // Actualizar solo las propiedades que se proporcionan en el request
        if (!string.IsNullOrEmpty(request.UserName))
            existingUser.UserName = request.UserName;

        if (!string.IsNullOrEmpty(request.Name))
            existingUser.Name = request.Name;

        if (!string.IsNullOrEmpty(request.LastName))
            existingUser.LastName = request.LastName;

        if (!string.IsNullOrEmpty(request.Status))
            existingUser.Status = request.Status;

        if (request.Aux != null) // Permitir empty string
            existingUser.Aux = request.Aux;

        if (!string.IsNullOrEmpty(request.Verified))
            existingUser.Verified = bool.TryParse(request.Verified, out var verified) ? verified : (bool?)null;

        if (request.IdRols > 0)
            existingUser.IdRol = request.IdRols;

        // ✅ ACTUALIZAR IdConfigSys SI SE PROPORCIONA
        if (request.IdConfigSys.HasValue && request.IdConfigSys.Value > 0)
            existingUser.IdConfigSys = request.IdConfigSys.Value;

        // La contraseña se maneja en el servicio, no aquí
        // CreateAt no se debe modificar en una actualización

        return existingUser;
    }

    // ✅ MÉTODO ToResponse PARA UserResponse
    public static UserResponse ToResponse(this User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return new UserResponse
        {
            IdUsers = user.IdUser,
            UserName = user.UserName,
            Name = user.Name,
            LastName = user.LastName,
            Status = user.Status,
            Aux = user.Aux,
            CreateAt = user.CreateAt?.ToString("yyyy-MM-dd"),
            Verified = user.Verified?.ToString(),
            IdRols = user.IdRol,
            RolName = user.Rol?.Name,
            // ✅ INCLUIR PROPIEDADES DE CONFIG SYS
            IdConfigSys = user.IdConfigSys,
            ConfigSysName = user.ConfigSys?.CompanyName,
            CompanyName = user.ConfigSys?.CompanyName,
            BranchName = user.ConfigSys?.BranchName,
            LogoUrl = user.ConfigSys?.LogoUrl,
            // Configuraciones y permisos
            Modules = user.Rol?.Modules ?? new List<Module>(),
            Permissions = user.Rol?.Permissions ?? new List<Permission>()
        };
    }

    // ✅ MÉTODO ToAuthResponse PARA AuthUserResponse
    public static AuthUserResponse ToAuthResponse(this User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return new AuthUserResponse
        {
            IdUsers = user.IdUser,
            UserName = user.UserName,
            Name = user.Name,
            LastName = user.LastName,
            Status = user.Status,
            Aux = user.Aux,
            Hash = user.HashPassword,
            CreateAt = user.CreateAt?.ToString("yyyy-MM-dd"),
            Verified = user.Verified?.ToString(),
            IdRols = user.IdRol,
            RolName = user.Rol?.Name,
            // ✅ INCLUIR PROPIEDADES DE CONFIG SYS
            IdConfigSys = user.IdConfigSys,
            ConfigSysName = user.ConfigSys?.CompanyName,
            CompanyName = user.ConfigSys?.CompanyName,
            BranchName = user.ConfigSys?.BranchName,
            LogoUrl = user.ConfigSys?.LogoUrl
        };
    }
}