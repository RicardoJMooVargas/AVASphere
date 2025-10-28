using Microsoft.Extensions.Logging;
using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Extensions;

namespace AVASphere.Infrastructure.Common.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserResponse> SearchUsersAsync(int idUsers)
    {
        try
        {
            _logger.LogInformation("Buscando usuario con ID: {IdUsers}", idUsers);

            var user = await _userRepository.SelectByIdAsync(idUsers);
            
            if (user == null)
            {
                _logger.LogWarning("Usuario con ID {IdUsers} no encontrado", idUsers);
                throw new KeyNotFoundException($"Usuario con ID {idUsers} no encontrado");
            }

            _logger.LogInformation("Usuario con ID {IdUsers} encontrado exitosamente", idUsers);
            return user.ToResponse();
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar usuario con ID {IdUsers}", idUsers);
            throw new ApplicationException($"Error al buscar el usuario: {ex.Message}", ex);
        }
    }

    public async Task<UserResponse> NewUsersAsync(UserCreateRequest request)
    {
        try
        {
            _logger.LogInformation("Creando nuevo usuario: {UserName}", request.UserName);

            // Validar que el UserName no esté duplicado
            var existingUser = await _userRepository.SelectUserAsync(new User { UserName = request.UserName });
            if (existingUser != null)
            {
                _logger.LogWarning("Intento de crear usuario con nombre duplicado: {UserName}", request.UserName);
                throw new InvalidOperationException($"El nombre de usuario '{request.UserName}' ya está en uso");
            }

            // Mapear request a entidad
            var user = request.ToEntity();

            await _userRepository.CreateUsersAsync(user);
            _logger.LogInformation("Usuario creado exitosamente con ID: {IdUsers}", user.IdUsers);

            return user.ToResponse();
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear nuevo usuario: {UserName}", request.UserName);
            throw new ApplicationException($"Error al crear el usuario: {ex.Message}", ex);
        }
    }

    public async Task<UserResponse> EditUsersAsync(UserUpdateRequest request)
    {
        try
        {
            _logger.LogInformation("Actualizando usuario con ID: {IdUsers}", request.IdUsers);

            // Verificar que el usuario existe
            var existingUser = await _userRepository.SelectByIdAsync(request.IdUsers);
            if (existingUser == null)
            {
                _logger.LogWarning("Intento de actualizar usuario inexistente con ID: {IdUsers}", request.IdUsers);
                throw new KeyNotFoundException($"Usuario con ID {request.IdUsers} no encontrado");
            }

            // Validar que el UserName no esté duplicado (si se está cambiando)
            if (!string.IsNullOrEmpty(request.UserName) && request.UserName != existingUser.UserName)
            {
                var userWithSameName = await _userRepository.SelectUserAsync(new User { UserName = request.UserName });
                if (userWithSameName != null && userWithSameName.IdUsers != request.IdUsers)
                {
                    _logger.LogWarning("Intento de cambiar a nombre de usuario duplicado: {UserName}", request.UserName);
                    throw new InvalidOperationException($"El nombre de usuario '{request.UserName}' ya está en uso");
                }
            }

            // Mapear request a entidad (preservando datos existentes)
            var user = request.ToEntity(existingUser);

            await _userRepository.UpdateUsersAsync(user);
            _logger.LogInformation("Usuario con ID {IdUsers} actualizado exitosamente", user.IdUsers);

            return user.ToResponse();
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario con ID: {IdUsers}", request.IdUsers);
            throw new ApplicationException($"Error al actualizar el usuario: {ex.Message}", ex);
        }
    }
}