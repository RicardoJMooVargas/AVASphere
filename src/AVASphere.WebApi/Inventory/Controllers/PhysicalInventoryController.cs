using AVASphere.ApplicationCore.Common.Enums;
using AVASphere.ApplicationCore.Inventory.DTOs;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AVASphere.WebApi.Inventory.Controllers;

[ApiController]
[Route("api/inventory/[controller]")]
[ApiExplorerSettings(GroupName = nameof(SystemModule.Inventory))]
[Tags("Inventory - Physical Inventory")]
public class PhysicalInventoryController : ControllerBase
{
    private readonly IPhysicalInventoryService _physicalInventoryService;

    public PhysicalInventoryController(IPhysicalInventoryService physicalInventoryService)
    {
        _physicalInventoryService = physicalInventoryService;
    }

    /// <summary>
    /// Crear un nuevo conteo físico
    /// </summary>
    /// <param name="createDto">Datos para crear el conteo físico</param>
    /// <returns>Conteo físico creado</returns>
    [HttpPost]
    public async Task<IActionResult> CreatePhysicalInventory([FromBody] CreatePhysicalInventoryDto createDto)
    {
        var result = await _physicalInventoryService.CreatePhysicalInventoryAsync(createDto);
        
        if (result.Success)
        {
            return CreatedAtAction(nameof(GetPhysicalInventoryById), 
                new { id = result.Data!.IdPhysicalInventory }, result);
        }
        
        return result.StatusCode switch
        {
            400 => BadRequest(result),
            404 => NotFound(result),
            500 => StatusCode(500, result),
            _ => BadRequest(result)
        };
    }

    /// <summary>
    /// Actualizar un conteo físico existente
    /// NOTA: Solo se permite editar IdWarehouse si no existen registros en PhysicalInventoryDetail
    /// </summary>
    /// <param name="updateDto">Datos para actualizar el conteo físico</param>
    /// <returns>Conteo físico actualizado</returns>
    [HttpPut]
    public async Task<IActionResult> UpdatePhysicalInventory([FromBody] UpdatePhysicalInventoryDto updateDto)
    {
        var result = await _physicalInventoryService.UpdatePhysicalInventoryAsync(updateDto);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return result.StatusCode switch
        {
            400 => BadRequest(result),
            404 => NotFound(result),
            500 => StatusCode(500, result),
            _ => BadRequest(result)
        };
    }

    /// <summary>
    /// Eliminar un conteo físico
    /// </summary>
    /// <param name="id">ID del conteo físico a eliminar</param>
    /// <param name="forceDelete">Si true, permite eliminación en cascada aunque existan detalles (default: false)</param>
    /// <returns>Resultado de la eliminación</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePhysicalInventory(int id, [FromQuery] bool forceDelete = false)
    {
        var result = await _physicalInventoryService.DeletePhysicalInventoryAsync(id, forceDelete);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return result.StatusCode switch
        {
            400 => BadRequest(result),
            404 => NotFound(result),
            500 => StatusCode(500, result),
            _ => BadRequest(result)
        };
    }

    /// <summary>
    /// Obtener un conteo físico con todos los productos relacionados al warehouse y área del usuario
    /// </summary>
    /// <param name="id">ID del conteo físico</param>
    /// <param name="userId">ID del usuario que realiza el conteo (para obtener su área)</param>
    /// <returns>Conteo físico con productos relacionados</returns>
    [HttpGet("{id}/with-products")]
    public async Task<IActionResult> GetPhysicalInventoryWithProducts(int id, [FromQuery] int userId)
    {
        var result = await _physicalInventoryService.GetPhysicalInventoryWithProductsAsync(id, userId);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return result.StatusCode switch
        {
            404 => NotFound(result),
            500 => StatusCode(500, result),
            _ => BadRequest(result)
        };
    }

    /// <summary>
    /// Obtener un conteo físico por ID
    /// </summary>
    /// <param name="id">ID del conteo físico</param>
    /// <returns>Conteo físico encontrado</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPhysicalInventoryById(int id)
    {
        var result = await _physicalInventoryService.GetPhysicalInventoryByIdAsync(id);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return result.StatusCode switch
        {
            404 => NotFound(result),
            500 => StatusCode(500, result),
            _ => BadRequest(result)
        };
    }

    /// <summary>
    /// Obtener todos los conteos físicos con filtros opcionales
    /// </summary>
    /// <param name="idWarehouse">Filtro por warehouse</param>
    /// <param name="status">Filtro por estado (Open, Closed, Cancelled)</param>
    /// <param name="startDate">Fecha de inicio del filtro</param>
    /// <param name="endDate">Fecha de fin del filtro</param>
    /// <returns>Lista de conteos físicos</returns>
    [HttpGet]
    public async Task<IActionResult> GetPhysicalInventories(
        [FromQuery] int? idWarehouse = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _physicalInventoryService.GetPhysicalInventoriesAsync(idWarehouse, status, startDate, endDate);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return result.StatusCode switch
        {
            500 => StatusCode(500, result),
            _ => BadRequest(result)
        };
    }
}

