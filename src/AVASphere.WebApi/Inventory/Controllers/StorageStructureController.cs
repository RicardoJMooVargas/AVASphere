namespace AVASphere.WebApi.Inventory.Controllers;

using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Enums;
using AVASphere.ApplicationCore.Inventory.DTOs;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/inventory/[controller]")]
[ApiExplorerSettings(GroupName = nameof(SystemModule.Inventory))]
[Tags("Inventory - Storage Structure")]
public class StorageStructureController : ControllerBase
{
    private readonly IStorageStructureService _storageStructureService;
    private readonly ILogger<StorageStructureController> _logger;

    public StorageStructureController(
        IStorageStructureService storageStructureService, 
        ILogger<StorageStructureController> logger)
    {
        _storageStructureService = storageStructureService;
        _logger = logger;
    }

    [HttpGet("get")]
    public async Task<ActionResult> GetStorageStructures(
        [FromQuery] int? id, 
        [FromQuery] string? codeRack,
        [FromQuery] int? warehouseId,
        [FromQuery] int? areaId)
    {
        try
        {
            if (id.HasValue)
            {
                // Caso 1: Buscar por ID
                _logger.LogInformation("Retrieving storage structure by ID: {Id}", id.Value);
                var storageStructure = await _storageStructureService.GetByIdAsync(id.Value);

                if (storageStructure == null)
                {
                    return NotFound(new ApiResponse($"Storage structure with ID {id.Value} not found", 404));
                }

                return Ok(new ApiResponse(storageStructure, "Storage structure retrieved successfully", 200));
            }
            else if (!string.IsNullOrWhiteSpace(codeRack))
            {
                // Caso 2: Buscar por código de rack
                _logger.LogInformation("Retrieving storage structure by code rack: {CodeRack}", codeRack);
                var storageStructure = await _storageStructureService.GetByCodeRackAsync(codeRack);

                if (storageStructure == null)
                {
                    return NotFound(new ApiResponse($"Storage structure with code rack '{codeRack}' not found", 404));
                }

                return Ok(new ApiResponse(storageStructure, "Storage structure retrieved successfully", 200));
            }
            else if (warehouseId.HasValue)
            {
                // Caso 3: Filtrar por almacén
                _logger.LogInformation("Retrieving storage structures by warehouse ID: {WarehouseId}", warehouseId.Value);
                var storageStructures = await _storageStructureService.GetByWarehouseIdAsync(warehouseId.Value);
                return Ok(new ApiResponse(storageStructures, "Storage structures retrieved successfully", 200));
            }
            else if (areaId.HasValue)
            {
                // Caso 4: Filtrar por área
                _logger.LogInformation("Retrieving storage structures by area ID: {AreaId}", areaId.Value);
                var storageStructures = await _storageStructureService.GetByAreaIdAsync(areaId.Value);
                return Ok(new ApiResponse(storageStructures, "Storage structures retrieved successfully", 200));
            }
            else
            {
                // Caso 5: Obtener todas las estructuras de almacenamiento
                _logger.LogInformation("Retrieving all storage structures");
                var storageStructures = await _storageStructureService.GetAllAsync();
                return Ok(new ApiResponse(storageStructures, "Storage structures retrieved successfully", 200));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving storage structures - ID: {Id}, CodeRack: {CodeRack}, WarehouseId: {WarehouseId}, AreaId: {AreaId}", 
                id, codeRack, warehouseId, areaId);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPost("new")]
    public async Task<ActionResult> CreateStorageStructure([FromBody] StorageStructureRequestDto storageStructureRequest)
    {
        try
        {
            _logger.LogInformation("Creating a new storage structure with code rack: {CodeRack}", storageStructureRequest.CodeRack);
            var createdStorageStructure = await _storageStructureService.CreateAsync(storageStructureRequest);

            return CreatedAtAction(nameof(GetStorageStructures), new { id = createdStorageStructure.IdStorageStructure },
                new ApiResponse(createdStorageStructure, "Storage structure created successfully", 201));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Related entity not found while creating storage structure: {CodeRack}", storageStructureRequest.CodeRack);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while creating storage structure: {CodeRack}", storageStructureRequest.CodeRack);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating storage structure with code rack: {CodeRack}", storageStructureRequest.CodeRack);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPost("new-multiple")]
    public async Task<ActionResult> CreateMultipleStorageStructures([FromBody] List<StorageStructureRequestDto> storageStructureRequests)
    {
        try
        {
            _logger.LogInformation("Creating {Count} storage structures", storageStructureRequests.Count);
            
            var createdStorageStructures = new List<StorageStructureResponseDto>();
            var errors = new List<string>();

            foreach (var request in storageStructureRequests)
            {
                try
                {
                    var created = await _storageStructureService.CreateAsync(request);
                    createdStorageStructures.Add(created);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error creating storage structure with code rack: {CodeRack}", request.CodeRack);
                    errors.Add($"Error creating storage structure '{request.CodeRack}': {ex.Message}");
                }
            }

            if (errors.Any())
            {
                return Ok(new ApiResponse(
                    new { created = createdStorageStructures, errors = errors },
                    $"Created {createdStorageStructures.Count} storage structures with {errors.Count} errors",
                    207)); // 207 Multi-Status
            }

            return Ok(new ApiResponse(createdStorageStructures, $"Successfully created {createdStorageStructures.Count} storage structures", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating multiple storage structures");
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpPut("edit/{id}")]
    public async Task<ActionResult> UpdateStorageStructure(int id, [FromBody] StorageStructureRequestDto storageStructureRequest)
    {
        try
        {
            _logger.LogInformation("Updating storage structure with ID: {Id}", id);
            var updatedStorageStructure = await _storageStructureService.UpdateAsync(id, storageStructureRequest);
            return Ok(new ApiResponse(updatedStorageStructure, "Storage structure updated successfully", 200));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Storage structure or related entity not found for update: {Id}", id);
            return NotFound(new ApiResponse(ex.Message, 404));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while updating storage structure: {Id}", id);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating storage structure with ID: {Id}", id);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteStorageStructure(int id)
    {
        try
        {
            _logger.LogInformation("Deleting storage structure with ID: {Id}", id);
            var result = await _storageStructureService.DeleteAsync(id);

            if (!result)
            {
                return NotFound(new ApiResponse($"Storage structure with ID {id} not found", 404));
            }

            return Ok(new ApiResponse(null, "Storage structure deleted successfully", 200));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while deleting storage structure: {Id}", id);
            return BadRequest(new ApiResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting storage structure with ID: {Id}", id);
            return StatusCode(500, new ApiResponse("Internal server error", 500));
        }
    }
}
