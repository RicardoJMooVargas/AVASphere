using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Enums;
using AVASphere.WebApi.Common.Extensions;

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
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetUser(int idUsers)
    {
        try
        {
            _logger.LogInformation("Solicitando usuario con ID: {IdUser}", idUsers);
            
            var user = await _userService.SearchUsersAsync(idUsers, null);
            return Ok(new ApiResponse(user, "Usuario encontrado exitosamente", 200));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Usuario con ID {IdUser} no encontrado", idUsers);
            return NotFound(new ApiResponse(ex.Message, 404));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario con ID {IdUser}", idUsers);
            return StatusCode(500, new ApiResponse("Error interno del servidor", 500));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreateUser(UserCreateRequest request)
    {
        try
        {
            _logger.LogInformation("Creando nuevo usuario: {UserName}", request.UserName);
        
            var user = await _userService.NewUsersAsync(request);
        
            return CreatedAtAction(nameof(GetUser), new { idUsers = user.IdUsers }, 
                new ApiResponse(user, "Usuario creado exitosamente", 201));
        }
        catch (InvalidOperationException opEx) when (opEx.Message.Contains("ya está en uso"))
        {
            _logger.LogWarning(opEx, "Intento de crear usuario duplicado: {UserName}", request.UserName);
            return Conflict(new ApiResponse(opEx.Message, 409));
        }
        catch (ArgumentException argEx) when (argEx.Message.Contains("contraseña"))
        {
            _logger.LogWarning(argEx, "Contraseña inválida para usuario: {UserName}", request.UserName);
            return BadRequest(new ApiResponse(argEx.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario: {UserName}", request.UserName);
            return StatusCode(500, new ApiResponse("Error interno del servidor", 500));
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
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateUser(int idUsers, UserUpdateRequest request)
    {
        try
        {
            // Asegurar que el ID de la ruta coincide con el del request
            if (idUsers != request.IdUsers)
            {
                return BadRequest(new ApiResponse("El ID de la ruta no coincide con el ID del usuario", 400));
            }

            _logger.LogInformation("Actualizando usuario con ID: {IdUser}", idUsers);
            
            var user = await _userService.EditUsersAsync(request);
            return Ok(new ApiResponse(user, "Usuario actualizado exitosamente", 200));
        }
        catch (KeyNotFoundException keyEx)
        {
            _logger.LogWarning(keyEx, "Usuario con ID {IdUser} no encontrado para actualizar", idUsers);
            return NotFound(new ApiResponse(keyEx.Message, 404));
        }
        catch (InvalidOperationException opEx) when (opEx.Message.Contains("ya está en uso"))
        {
            _logger.LogWarning(opEx, "Intento de actualizar a nombre de usuario duplicado");
            return Conflict(new ApiResponse(opEx.Message, 409));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario con ID {IdUser}", idUsers);
            return StatusCode(500, new ApiResponse("Error interno del servidor", 500));
        }
    }

    /// <summary>
    /// Opciones preflight para CORS
    /// </summary>
    [HttpOptions]
    public ActionResult Options()
    {
        Response.Headers.Append("Allow", "GET,POST,PUT,OPTIONS");
        return Ok(new ApiResponse(null, "Options request successful", 200));
    }

    [HttpPost("setup-admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SetupAdminUser([FromBody] AdminSetupRequest request)
    {
        try
        {
            _logger.LogInformation("Configurando usuario administrador");

            if (request == null || string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ApiResponse("UserName y Password son requeridos", 400));
            }

            var user = await _userService.SetupAdminUserAsync(request.UserName, request.Password);

            return CreatedAtAction(nameof(GetUser), new { idUsers = user.IdUsers }, 
                new ApiResponse(user, "Usuario administrador configurado exitosamente", 201));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de negocio al configurar admin");
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al configurar usuario administrador");
            return StatusCode(500, new ApiResponse("Error interno del servidor", 500));
        }
    }
}