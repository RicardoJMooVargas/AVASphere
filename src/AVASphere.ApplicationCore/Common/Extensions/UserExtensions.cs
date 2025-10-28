using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.DTOs;

namespace AVASphere.ApplicationCore.Common.Extensions;

public static class UserExtensions
{
    public static User ToEntity(this UserCreateRequest request)
    {
        return new User
        {
            UserName = request.UserName,
            Name = request.Name,
            LastName = request.LastName,
            Password = request.Password,
            HashPassword = request.HashPassword,
            Status = request.Status ?? "Active",
            Aux = request.Aux,
            Verified = request.Verified,
            IdRol = request.IdRols,
            CreateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }

    public static User ToEntity(this UserUpdateRequest request, User existingUser)
    {
        // Actualizar solo los campos que vienen en el request
        return new User
        {
            IdUsers = request.IdUsers,
            UserName = !string.IsNullOrEmpty(request.UserName) ? request.UserName : existingUser.UserName,
            Name = request.Name ?? existingUser.Name,
            LastName = request.LastName ?? existingUser.LastName,
            Password = request.Password ?? existingUser.Password,
            HashPassword = request.HashPassword ?? existingUser.HashPassword,
            Status = request.Status ?? existingUser.Status,
            Aux = request.Aux ?? existingUser.Aux,
            Verified = request.Verified ?? existingUser.Verified,
            IdRol = request.IdRols ?? existingUser.IdRol,
            CreateAt = existingUser.CreateAt // No se actualiza
        };
    }

    public static UserResponse ToResponse(this User user)
    {
        if (user == null) return null;

        return new UserResponse
        {
            IdUsers = user.IdUsers,
            UserName = user.UserName,
            Name = user.Name,
            LastName = user.LastName,
            Status = user.Status,
            Aux = user.Aux,
            CreateAt = user.CreateAt,
            Verified = user.Verified,
            IdRols = user.IdRol,
            RolName = user.Rol?.Name // Asumiendo que Rol tiene una propiedad Name
        };
    }
}