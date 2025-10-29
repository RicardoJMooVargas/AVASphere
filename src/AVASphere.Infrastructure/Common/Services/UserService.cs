using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Extensions;

namespace AVASphere.Infrastructure.Common.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, IEncryptionService encryptionService, ILogger<UserService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserResponse> SearchUsersAsync(int? idUsers = null, string? userName = null)
    {
        try
        {
            if (idUsers == null && string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("Se requiere al menos un criterio de búsqueda: idUsers o userName");

            _logger.LogInformation("Iniciando búsqueda de usuario (ID={IdUser}, UserName={UserName})", idUsers, userName);

            // 🔹 Construimos el objeto User con los criterios disponibles
            var userCriteria = new User();

            if (idUsers.HasValue)
            {
                // 🔸 Si hay ID, prioriza búsqueda por ID
                userCriteria.IdUser = idUsers.Value;
            }
            else
            {
                userCriteria.UserName = userName!.Trim();
            }

            // 🔍 Llamada al repositorio
            var user = await _userRepository.SelectUserAsync(userCriteria);

            if (user == null)
            {
                string criterio = idUsers.HasValue ? $"ID {idUsers}" : $"UserName '{userName}'";
                _logger.LogWarning("Usuario con {Criterio} no encontrado", criterio);
                throw new KeyNotFoundException($"Usuario con {criterio} no encontrado");
            }

            _logger.LogInformation("Usuario encontrado correctamente (ID={IdUser}, UserName={UserName})", user.IdUser, user.UserName);
            return user.ToResponse();
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar usuario (ID={IdUser}, UserName={UserName})", idUsers, userName);
            throw new ApplicationException($"Error al buscar el usuario: {ex.Message}", ex);
        }
    }



    public async Task<UserResponse> NewUsersAsync(UserCreateRequest request)
    {
        try
        {
            _logger.LogInformation("Creando nuevo usuario: {UserName}", request.UserName);

            // Validaciones básicas
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.UserName))
                throw new ArgumentException("El nombre de usuario es requerido");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("La contraseña es requerida");

            // Validar que el UserName no esté duplicado
            var existingUser = await _userRepository.SelectUserAsync(new User { UserName = request.UserName });
            if (existingUser != null)
            {
                _logger.LogWarning("Intento de crear usuario con nombre duplicado: {UserName}", request.UserName);
                throw new InvalidOperationException($"El nombre de usuario '{request.UserName}' ya está en uso");
            }

            // Mapear request a entidad
            var user = request.ToEntity();

            // Encriptar la contraseña
            user.HashPassword = _encryptionService.HashPassword(request.Password);
            
            // Limpiar la contraseña en texto plano (no almacenarla)
            user.Password = null;

            // Establecer valores por defecto
            user.Status = !string.IsNullOrWhiteSpace(user.Status) ? user.Status : "Active";
            user.Verified = !string.IsNullOrWhiteSpace(user.Verified) ? user.Verified : "No";
            user.CreateAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            await _userRepository.CreateUsersAsync(user);
            _logger.LogInformation("Usuario creado exitosamente con ID: {IdUser}", user.IdUser);

            return user.ToResponse();
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (ArgumentException)
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
            _logger.LogInformation("Actualizando usuario con ID: {IdUser}", request.IdUsers);

            // Verificar que el usuario existe
            var existingUser = await _userRepository.SelectByIdAsync(request.IdUsers);
            if (existingUser == null)
            {
                _logger.LogWarning("Intento de actualizar usuario inexistente con ID: {IdUser}", request.IdUsers);
                throw new KeyNotFoundException($"Usuario con ID {request.IdUsers} no encontrado");
            }

            // Validar que el UserName no esté duplicado (si se está cambiando)
            if (!string.IsNullOrEmpty(request.UserName) && request.UserName != existingUser.UserName)
            {
                var userWithSameName = await _userRepository.SelectUserAsync(new User { UserName = request.UserName });
                if (userWithSameName != null && userWithSameName.IdUser != request.IdUsers)
                {
                    _logger.LogWarning("Intento de cambiar a nombre de usuario duplicado: {UserName}", request.UserName);
                    throw new InvalidOperationException($"El nombre de usuario '{request.UserName}' ya está en uso");
                }
            }

            // Mapear request a entidad (preservando datos existentes)
            var user = request.ToEntity(existingUser);

            // Si se proporciona una nueva contraseña, encriptarla
            if (!string.IsNullOrEmpty(request.Password))
            {
                user.HashPassword = _encryptionService.HashPassword(request.Password);
                user.Password = null; // No almacenar en texto plano
            }
            else
            {
                // Preservar el hash de contraseña existente
                user.HashPassword = existingUser.HashPassword;
            }

            await _userRepository.UpdateUsersAsync(user);
            _logger.LogInformation("Usuario con ID {IdUser} actualizado exitosamente", user.IdUser);

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
            _logger.LogError(ex, "Error al actualizar usuario con ID: {IdUser}", request.IdUsers);
            throw new ApplicationException($"Error al actualizar el usuario: {ex.Message}", ex);
        }
    }
    public async Task<LoginResponse> AuthenticateUserAsync(LoginRequest loginRequest)
{
    try
    {
        _logger.LogInformation("Intentando autenticar usuario: {UserName}", loginRequest.UserName);

        // Validaciones básicas
        if (loginRequest == null)
            throw new ArgumentNullException(nameof(loginRequest));

        if (string.IsNullOrWhiteSpace(loginRequest.UserName))
            throw new ArgumentException("El nombre de usuario es requerido");

        if (string.IsNullOrWhiteSpace(loginRequest.Password))
            throw new ArgumentException("La contraseña es requerida");

        // Buscar usuario por nombre de usuario
        var user = await _userRepository.SelectUserAsync(new User { UserName = loginRequest.UserName });
        
        if (user == null)
        {
            _logger.LogWarning("Intento de login con usuario inexistente: {UserName}", loginRequest.UserName);
            return LoginResponse.FailureResponse("Credenciales inválidas");
        }

        // Verificar estado del usuario
        if (user.Status != "Active")
        {
            _logger.LogWarning("Intento de login con usuario inactivo: {UserName}", loginRequest.UserName);
            return LoginResponse.FailureResponse("La cuenta de usuario está deshabilitada");
        }

        // Verificar contraseña
        if (string.IsNullOrEmpty(user.HashPassword) || 
            !_encryptionService.VerifyPassword(loginRequest.Password, user.HashPassword))
        {
            _logger.LogWarning("Contraseña incorrecta para usuario: {UserName}", loginRequest.UserName);
            return LoginResponse.FailureResponse("Credenciales inválidas");
        }

        // Generar token (si tienes un servicio de tokens)
        string token = string.Empty; // Aquí llamarías a tu _tokenService.GenerateToken(user);
        
        _logger.LogInformation("Autenticación exitosa para usuario: {UserName}", loginRequest.UserName);
        
        return LoginResponse.SuccessResponse(token, user.ToResponse());
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error durante la autenticación del usuario: {UserName}", loginRequest.UserName);
        return LoginResponse.FailureResponse("Error interno durante la autenticación");
    }
}
}