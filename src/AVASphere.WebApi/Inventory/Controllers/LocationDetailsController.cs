using System.Security.Claims;
using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Inventory.DTOs;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AVASphere.WebApi.Inventory.Controllers;

[ApiController]
[Route("api/inventory/[controller]")]
[ApiExplorerSettings(GroupName = "Inventory")]
[Tags("Inventory - Location Details")]
[Authorize] // Requiere autenticación JWT
public class LocationDetailsController : ControllerBase
{
    private readonly ILocationDetailsService _locationDetailsService;
    private readonly IUserService _userService;
    private readonly IRolService _rolService;
    private readonly ILogger<LocationDetailsController> _logger;

    public LocationDetailsController(
        ILocationDetailsService locationDetailsService,
        IUserService userService,
        IRolService rolService,
        ILogger<LocationDetailsController> logger)
    {
        _locationDetailsService = locationDetailsService;
        _userService = userService;
        _rolService = rolService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene los detalles de ubicación por ID
    /// </summary>
    /// <param name="id">ID de la ubicación</param>
    /// <returns>Detalles de la ubicación con información de área y estructura de almacenamiento</returns>
    /// <response code="200">Ubicación encontrada exitosamente</response>
    /// <response code="404">Ubicación no encontrada</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<LocationDetailsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LocationDetailsResponseDto>>> GetById(int id)
    {
        try
        {
            _logger.LogInformation("Obteniendo ubicación con ID: {Id}", id);

            var location = await _locationDetailsService.GetByIdAsync(id);
            if (location == null)
            {
                return NotFound(new ApiResponse<LocationDetailsResponseDto>($"Ubicación con ID {id} no encontrada", 404));
            }

            return Ok(new ApiResponse<LocationDetailsResponseDto>(location, "Ubicación encontrada exitosamente", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en operación: GetLocationDetailsById");
            return StatusCode(500, new ApiResponse<LocationDetailsResponseDto>("Error interno del servidor", 500));
        }
    }

    /// <summary>
    /// Crea una nueva ubicación de almacenamiento
    /// </summary>
    /// <param name="request">Datos de la nueva ubicación</param>
    /// <returns>Ubicación creada</returns>
    /// <response code="201">Ubicación creada exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="409">La ubicación ya existe con los mismos parámetros</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LocationDetailsResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LocationDetailsResponseDto>>> Create([FromBody] LocationDetailsRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(new ApiResponse("Datos de entrada inválidos", 400, errors));
            }

            _logger.LogInformation("Creando nueva ubicación");

            // Obtener IdArea del usuario actual si no se especifica
            int? userAreaId = null;
            if (!request.IdArea.HasValue)
            {
                userAreaId = await GetUserAreaIdAsync();
                if (!userAreaId.HasValue)
                {
                    return BadRequest(new ApiResponse("No se puede determinar el área del usuario. Debe especificar IdArea explícitamente.", 400));
                }
            }

            var created = await _locationDetailsService.CreateAsync(request, userAreaId);
            
            return CreatedAtAction(nameof(GetById), new { id = created.IdLocationDetails }, 
                new ApiResponse<LocationDetailsResponseDto>(created, "Ubicación creada exitosamente", 201));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiResponse(ex.Message, 409));
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en operación: CreateLocationDetails");
            return StatusCode(500, new ApiResponse("Error interno del servidor", 500));
        }
    }

    /// <summary>
    /// Actualiza una ubicación existente
    /// </summary>
    /// <param name="id">ID de la ubicación a actualizar</param>
    /// <param name="request">Nuevos datos de la ubicación</param>
    /// <returns>Ubicación actualizada</returns>
    /// <response code="200">Ubicación actualizada exitosamente</response>
    /// <response code="400">Datos de entrada inválidos</response>
    /// <response code="404">Ubicación no encontrada</response>
    /// <response code="409">Conflicto con ubicación existente</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<LocationDetailsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LocationDetailsResponseDto>>> Update(int id, [FromBody] LocationDetailsUpdateDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(new ApiResponse("Datos de entrada inválidos", 400, errors));
            }

            _logger.LogInformation("Actualizando ubicación con ID: {Id}", id);

            var updated = await _locationDetailsService.UpdateAsync(id, request);
            return Ok(new ApiResponse<LocationDetailsResponseDto>(updated, "Ubicación actualizada exitosamente", 200));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse<LocationDetailsResponseDto>($"Ubicación con ID {id} no encontrada", 404));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiResponse(ex.Message, 409));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en operación: UpdateLocationDetails");
            return StatusCode(500, new ApiResponse("Error interno del servidor", 500));
        }
    }

    /// <summary>
    /// Elimina una ubicación
    /// </summary>
    /// <param name="id">ID de la ubicación a eliminar</param>
    /// <returns>Resultado de la eliminación</returns>
    /// <response code="200">Ubicación eliminada exitosamente</response>
    /// <response code="404">Ubicación no encontrada</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        try
        {
            _logger.LogInformation("Eliminando ubicación con ID: {Id}", id);

            var deleted = await _locationDetailsService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new ApiResponse($"Ubicación con ID {id} no encontrada", 404));
            }

            return Ok(new ApiResponse("Ubicación eliminada exitosamente. Los inventarios físicos relacionados han sido desvinculados.", 200));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse($"Ubicación con ID {id} no encontrada", 404));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en operación: DeleteLocationDetails");
            return StatusCode(500, new ApiResponse("Error interno del servidor", 500));
        }
    }

    /// <summary>
    /// Obtiene todas las ubicaciones
    /// </summary>
    /// <returns>Lista de todas las ubicaciones</returns>
    /// <response code="200">Lista obtenida exitosamente</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LocationDetailsResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LocationDetailsResponseDto>>>> GetAll()
    {
        try
        {
            _logger.LogInformation("Obteniendo todas las ubicaciones");

            var locations = await _locationDetailsService.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<LocationDetailsResponseDto>>(locations, "Ubicaciones obtenidas exitosamente", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en operación: GetAllLocationDetails");
            return StatusCode(500, new ApiResponse("Error interno del servidor", 500));
        }
    }

    /// <summary>
    /// Obtiene ubicaciones por área
    /// </summary>
    /// <param name="areaId">ID del área</param>
    /// <returns>Lista de ubicaciones del área especificada</returns>
    /// <response code="200">Lista obtenida exitosamente</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("by-area/{areaId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LocationDetailsResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LocationDetailsResponseDto>>>> GetByAreaId(int areaId)
    {
        try
        {
            _logger.LogInformation("Obteniendo ubicaciones por área: {AreaId}", areaId);

            var locations = await _locationDetailsService.GetByAreaIdAsync(areaId);
            return Ok(new ApiResponse<IEnumerable<LocationDetailsResponseDto>>(locations, "Ubicaciones por área obtenidas exitosamente", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en operación: GetLocationDetailsByArea");
            return StatusCode(500, new ApiResponse("Error interno del servidor", 500));
        }
    }

    /// <summary>
    /// Obtiene ubicaciones por estructura de almacenamiento
    /// </summary>
    /// <param name="storageStructureId">ID de la estructura de almacenamiento</param>
    /// <returns>Lista de ubicaciones de la estructura especificada</returns>
    /// <response code="200">Lista obtenida exitosamente</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("by-storage-structure/{storageStructureId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LocationDetailsResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LocationDetailsResponseDto>>>> GetByStorageStructureId(int storageStructureId)
    {
        try
        {
            _logger.LogInformation("Obteniendo ubicaciones por estructura de almacenamiento: {StorageStructureId}", storageStructureId);

            var locations = await _locationDetailsService.GetByStorageStructureIdAsync(storageStructureId);
            return Ok(new ApiResponse<IEnumerable<LocationDetailsResponseDto>>(locations, "Ubicaciones por estructura de almacenamiento obtenidas exitosamente", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en operación: GetLocationDetailsByStorageStructure");
            return StatusCode(500, new ApiResponse("Error interno del servidor", 500));
        }
    }

    /// <summary>
    /// Busca una ubicación por parámetros específicos
    /// </summary>
    /// <param name="areaId">ID del área</param>
    /// <param name="storageStructureId">ID de la estructura de almacenamiento</param>
    /// <param name="section">Sección (A o B)</param>
    /// <param name="verticalLevel">Nivel vertical</param>
    /// <returns>Ubicación que coincide con los parámetros</returns>
    /// <response code="200">Ubicación encontrada</response>
    /// <response code="404">Ubicación no encontrada</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<LocationDetailsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LocationDetailsResponseDto>>> SearchByParameters(
        [FromQuery] int areaId,
        [FromQuery] int storageStructureId,
        [FromQuery] string section,
        [FromQuery] int verticalLevel)
    {
        try
        {
            _logger.LogInformation("Buscando ubicación por parámetros: Área={AreaId}, Estructura={StorageStructureId}, Sección={Section}, Nivel={VerticalLevel}", 
                areaId, storageStructureId, section, verticalLevel);

            if (string.IsNullOrWhiteSpace(section))
            {
                return BadRequest(new ApiResponse("El parámetro 'section' es requerido", 400));
            }

            var location = await _locationDetailsService.GetByLocationParametersAsync(areaId, storageStructureId, section, verticalLevel);
            if (location == null)
            {
                var searchParams = $"Área={areaId}, Estructura={storageStructureId}, Sección={section}, Nivel={verticalLevel}";
                return NotFound(new ApiResponse<LocationDetailsResponseDto>($"Ubicación no encontrada con parámetros: {searchParams}", 404));
            }

            return Ok(new ApiResponse<LocationDetailsResponseDto>(location, "Ubicación encontrada por parámetros", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en operación: SearchLocationDetailsByParameters");
            return StatusCode(500, new ApiResponse("Error interno del servidor", 500));
        }
    }

    /// <summary>
    /// Obtiene el IdArea del usuario actual basado en su rol
    /// </summary>
    /// <returns>IdArea del usuario o null si no se puede determinar</returns>
    private async Task<int?> GetUserAreaIdAsync()
    {
        try
        {
            // Obtener el ID del usuario del token JWT
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning("No se pudo obtener el ID del usuario del token JWT");
                return null;
            }

            // Obtener la información del usuario
            var users = await _userService.SearchUsersAsync(userId, null);
            var user = users.FirstOrDefault();
            
            if (user == null)
            {
                _logger.LogWarning("Usuario con ID {UserId} no encontrado", userId);
                return null;
            }
            
            // Obtener la información del rol del usuario usando la propiedad correcta
            int userRolId = user.IdRols; // Guardamos en variable para evitar múltiples enumeraciones
            var rol = await _rolService.GetByIdAsync(userRolId);
            if (rol == null)
            {
                _logger.LogWarning("Rol con ID {RolId} no encontrado para usuario {UserId}", userRolId, userId);
                return null;
            }

            _logger.LogInformation("IdArea {AreaId} obtenida para usuario {UserId} con rol {RolName}", rol.IdArea, userId, rol.Name);
            return rol.IdArea;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el IdArea del usuario actual");
            return null;
        }
    }
}