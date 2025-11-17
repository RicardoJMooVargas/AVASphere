namespace AVASphere.WebApi.Common.Controllers;

using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Enums;
using AVASphere.ApplicationCore.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/common/[controller]")]
[ApiExplorerSettings(GroupName = nameof(SystemModule.Common))]
[Tags("Common - Roles")]
public class RolController : ControllerBase
{
    private readonly IRolService _rolService;
    private readonly ILogger<RolController> _logger;
    
    public RolController(IRolService rolService, ILogger<RolController> logger)
    {
        _rolService = rolService;
        _logger = logger;
    }
    
    [HttpGet("get")]
    public async Task<ActionResult> GetRoles([FromQuery] int? id, [FromQuery] string? name)
    {
        try
        {
            if (id.HasValue)
            {
                // Caso 1: Buscar por ID
                _logger.LogInformation("Retrieving rol by ID: {RolId}", id.Value);
                var rol = await _rolService.GetByIdAsync(id.Value);
                
                if (rol == null)
                {
                    return NotFound(new ApiResponse($"Rol with ID {id.Value} not found", 404));
                }
                
                return Ok(new ApiResponse(rol, "Rol retrieved successfully", 200));
            }
            else if (!string.IsNullOrWhiteSpace(name))
            {
                // Caso 2: Buscar por nombre
                _logger.LogInformation("Retrieving rol by name: {RolName}", name);
                var rol = await _rolService.GetByNameAsync(name);
                
                if (rol == null)
                {
                    return NotFound(new ApiResponse($"Rol with name '{name}' not found", 404));
                }
                
                return Ok(new ApiResponse(rol, "Rol retrieved successfully", 200));
            }
            else
            {
                // Caso 3: Obtener todos los roles
                _logger.LogInformation("Retrieving all roles");
                var roles = await _rolService.GetAllAsync();
                return Ok(new ApiResponse(roles, "Roles retrieved successfully", 200));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles - ID: {RolId}, Name: {RolName}", id, name);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPost("new")]
    public async Task<ActionResult> CreateRol([FromBody] RolRequestDto rolRequest)
    {
        try
        {
            _logger.LogInformation("Creating a new rol with name: {RolName}", rolRequest.Name);
            var createdRol = await _rolService.CreateAsync(rolRequest);
            
            return CreatedAtAction(nameof(GetRoles), new { id = createdRol.IdRol }, 
                new ApiResponse(createdRol, "Rol created successfully", 201));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Area not found while creating rol: {RolName}", rolRequest.Name);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while creating rol: {RolName}", rolRequest.Name);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating rol with name: {RolName}", rolRequest.Name);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPut("edit/{id}")]
    public async Task<ActionResult> UpdateRol(int id, [FromBody] RolRequestDto rolRequest)
    {
        try
        {
            _logger.LogInformation("Updating rol with ID: {RolId}", id);
            var updatedRol = await _rolService.UpdateAsync(id, rolRequest);
            return Ok(new ApiResponse(updatedRol, "Rol updated successfully", 200));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Rol or Area not found for update: {RolId}", id);
            return NotFound(new ApiResponse(ex.Message, 404));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while updating rol: {RolId}", id);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating rol with ID: {RolId}", id);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRol(int id)
    {
        try
        {
            _logger.LogInformation("Deleting rol with ID: {RolId}", id);
            var result = await _rolService.DeleteAsync(id);
            
            if (!result)
            {
                return NotFound(new ApiResponse($"Rol with ID {id} not found", 404));
            }
            
            return Ok(new ApiResponse(null, "Rol deleted successfully", 200));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while deleting rol: {RolId}", id);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting rol with ID: {RolId}", id);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }
}