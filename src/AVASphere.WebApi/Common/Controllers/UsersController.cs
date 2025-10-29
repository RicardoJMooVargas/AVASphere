using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Enums;

namespace AVASphere.WebApi.Common.Controllers;

[ApiController]
[Route("api/common/[controller]")]
[ApiExplorerSettings(GroupName = nameof(SystemModule.Common))]
[Tags("Common - User")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    /// <param name="idUsers">ID del usuario a buscar</param>
    /// <returns>Información del usuario encontrado</returns>
    /// <response code="200">Usuario encontrado exitosamente</response>
    /// <response code="404">Usuario no encontrado</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("{idUsers:int}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponse>> GetUser(int idUsers)
    {
        try
        {
            _logger.LogInformation("Solicitando usuario con ID: {IdUser}", idUsers);
            
            var user = await _userService.SearchUsersAsync(idUsers, null);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Usuario con ID {IdUser} no encontrado", idUsers);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario con ID {IdUser}", idUsers);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponse>> CreateUser(UserCreateRequest request)
    {
        try
        {
            _logger.LogInformation("Creando nuevo usuario: {UserName}", request.UserName);
        
            var user = await _userService.NewUsersAsync(request);
        
            return CreatedAtAction(
                nameof(GetUser), 
                new { idUsers = user.IdUsers }, 
                user
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("ya está en uso"))
        {
            _logger.LogWarning(ex, "Intento de crear usuario duplicado: {UserName}", request.UserName);
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex) when (ex.Message.Contains("contraseña"))
        {
            _logger.LogWarning(ex, "Contraseña inválida para usuario: {UserName}", request.UserName);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario: {UserName}", request.UserName);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza un usuario existente
    /// </summary>
    /// <param name="idUsers">ID del usuario a actualizar</param>
    /// <param name="request">Datos actualizados del usuario</param>
    /// <returns>Usuario actualizado</returns>
    /// <response code="200">Usuario actualizado exitosamente</response>
    /// <response code="400">Solicitud inválida</response>
    /// <response code="404">Usuario no encontrado</response>
    /// <response code="409">El nombre de usuario ya existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPut("{idUsers:int}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponse>> UpdateUser(int idUsers, UserUpdateRequest request)
    {
        try
        {
            // Asegurar que el ID de la ruta coincide con el del request
            if (idUsers != request.IdUsers)
            {
                return BadRequest(new { message = "El ID de la ruta no coincide con el ID del usuario" });
            }

            _logger.LogInformation("Actualizando usuario con ID: {IdUser}", idUsers);
            
            var user = await _userService.EditUsersAsync(request);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Usuario con ID {IdUser} no encontrado para actualizar", idUsers);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("ya está en uso"))
        {
            _logger.LogWarning(ex, "Intento de actualizar a nombre de usuario duplicado");
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario con ID {IdUser}", idUsers);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Opciones preflight para CORS
    /// </summary>
    [HttpOptions]
    public IActionResult Options()
    {
        Response.Headers.Append("Allow", "GET,POST,PUT,OPTIONS");
        return Ok();
    }
}