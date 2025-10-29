using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Enums;
using AVASphere.ApplicationCore.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AVASphere.WebApi.Common.Controllers;

[ApiController]
[Route("api/common/[controller]")]
[ApiExplorerSettings(GroupName = nameof(SystemModule.Common))]
[Tags("Common - Areas")]
public class AreaController : ControllerBase
{
    private readonly IAreaService _areaService;
    private readonly ILogger<AreaController> _logger;
    
    public AreaController(IAreaService areaService, ILogger<AreaController> logger)
    {
        _areaService = areaService;
        _logger = logger;
    }
    
    [HttpPost("new")]
    public async Task<IActionResult> NewArea([FromBody] AreaRequestDto areaRequest)
    {
        try
        {
            _logger.LogInformation("Creating a new area with name: {AreaName}", areaRequest.Name);
            var createdArea = await _areaService.CreateAsync(areaRequest);
            
            // Corrección: Usar CreatedAtAction con el método existente
            return CreatedAtAction(nameof(GetAreas), new { id = createdArea.IdArea }, createdArea);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while creating area: {AreaName}", areaRequest.Name);
            return BadRequest(ex.Message);  
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating area with name: {AreaName}", areaRequest.Name);
            return StatusCode(500, "Internal server error");
        }
    }
    [HttpGet("get")]
    public async Task<IActionResult> GetAreas([FromQuery] int? id, [FromQuery] string? name)
    {
        try
        {
            if (id.HasValue)
            {
                _logger.LogInformation("Retrieving area with ID: {AreaId}", id);
                var area = await _areaService.GetByIdAsync(id.Value);
                if (area == null)
                    return NotFound($"Area with ID {id} not found");
            
                return Ok(area);
            }

            if (!string.IsNullOrEmpty(name))
            {
                _logger.LogInformation("Retrieving area with name: {AreaName}", name);
                var area = await _areaService.GetByNameAsync(name);
                if (area == null)
                    return NotFound($"Area with name '{name}' not found");
            
                return Ok(area);
            }

            _logger.LogInformation("Retrieving all areas");
            var areas = await _areaService.GetAllAsync();
            return Ok(areas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving areas");
            return StatusCode(500, "Internal server error");
        }
    }


    [HttpPut("edit/{id}")]
    public async Task<IActionResult> UpdateArea(int id, [FromBody] AreaRequestDto areaRequest)
    {
        try
        {
            _logger.LogInformation("Updating area with ID: {AreaId}", id);
            var updatedArea = await _areaService.UpdateAsync(id, areaRequest);
            return Ok(updatedArea);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Area not found for update: {AreaId}", id);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while updating area: {AreaId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating area with ID: {AreaId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteArea(int id)
    {
        try
        {
            _logger.LogInformation("Deleting area with ID: {AreaId}", id);
            var result = await _areaService.DeleteAsync(id);
            
            if (!result)
            {
                return NotFound($"Area with ID {id} not found");
            }
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while deleting area: {AreaId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting area with ID: {AreaId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}