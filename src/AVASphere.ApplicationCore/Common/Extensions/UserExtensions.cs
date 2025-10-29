using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Entities.General;

namespace AVASphere.ApplicationCore.Common.Extensions;

public static class UserExtensions
{
    // Para UserCreateRequest
    public static User ToEntity(this UserCreateRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        return new User
        {
            UserName = request.UserName ?? throw new ArgumentNullException(nameof(request.UserName)),
            Name = request.Name,
            LastName = request.LastName,
            Password = null, // No almacenar contraseña en texto plano
            HashPassword = null, // Se establecerá en el servicio
            Status = "Active",
            Aux = request.Aux,
            Verified = request.Verified ?? "No",
            IdRol = request.IdRols
        };
    }

    // Para UserUpdateRequest - método CORREGIDO
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
            existingUser.Verified = request.Verified;

        if (request.IdRols > 0)
            existingUser.IdRol = request.IdRols;

        // La contraseña se maneja en el servicio, no aquí
        // CreateAt no se debe modificar en una actualización

        return existingUser;
    }

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
            CreateAt = user.CreateAt,
            Verified = user.Verified,
            IdRols = user.IdRol,
            RolName = user.Rol?.Name
        };
    }
}