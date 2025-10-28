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
    public async Task<IActionResult> GetRoles([FromQuery] int? id, [FromQuery] string? name)
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
                    return NotFound($"Rol with ID {id.Value} not found");
                }
                
                return Ok(rol);
            }
            else if (!string.IsNullOrWhiteSpace(name))
            {
                // Caso 2: Buscar por nombre
                _logger.LogInformation("Retrieving rol by name: {RolName}", name);
                var rol = await _rolService.GetByNameAsync(name);
                
                if (rol == null)
                {
                    return NotFound($"Rol with name '{name}' not found");
                }
                
                return Ok(rol);
            }
            else
            {
                // Caso 3: Obtener todos los roles
                _logger.LogInformation("Retrieving all roles");
                var roles = await _rolService.GetAllAsync();
                return Ok(roles);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles - ID: {RolId}, Name: {RolName}", id, name);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("new")]
    public async Task<IActionResult> CreateRol([FromBody] RolRequestDto rolRequest)
    {
        try
        {
            _logger.LogInformation("Creating a new rol with name: {RolName}", rolRequest.Name);
            var createdRol = await _rolService.CreateAsync(rolRequest);
            
            // Retornar 201 Created con la ubicación del nuevo recurso
            return CreatedAtAction(nameof(GetRoles), new { id = createdRol.IdRol }, createdRol);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Area not found while creating rol: {RolName}", rolRequest.Name);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while creating rol: {RolName}", rolRequest.Name);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating rol with name: {RolName}", rolRequest.Name);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("edit/{id}")]
    public async Task<IActionResult> UpdateRol(int id, [FromBody] RolRequestDto rolRequest)
    {
        try
        {
            _logger.LogInformation("Updating rol with ID: {RolId}", id);
            var updatedRol = await _rolService.UpdateAsync(id, rolRequest);
            return Ok(updatedRol);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Rol or Area not found for update: {RolId}", id);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while updating rol: {RolId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating rol with ID: {RolId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRol(int id)
    {
        try
        {
            _logger.LogInformation("Deleting rol with ID: {RolId}", id);
            var result = await _rolService.DeleteAsync(id);
            
            if (!result)
            {
                return NotFound($"Rol with ID {id} not found");
            }
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while deleting rol: {RolId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting rol with ID: {RolId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}