using AVASphere.ApplicationCore.Common.Enums;
using AVASphere.ApplicationCore.Inventory.DTOs;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;   

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
    [Authorize]
    public async Task<IActionResult> CreatePhysicalInventory([FromBody] CreatePhysicalInventoryDto createDto)
    {
        // Obtener el ID del usuario del token JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Token de autorización inválido o usuario no encontrado.");
        }

        var result = await _physicalInventoryService.CreatePhysicalInventoryAsync(createDto, userId);
        
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

    /// <summary>
    /// Obtener lista de productos para conteo físico basado en el IdPhysicalInventory
    /// Obtiene todos los PhysicalInventoryDetail asociados al inventario físico especificado.
    /// El userId se obtiene automáticamente del token JWT.
    /// </summary>
    /// <param name="idPhysicalInventory">ID del inventario físico para obtener sus productos</param>
    /// <returns>Lista de productos para realizar el conteo físico</returns>
    [HttpGet("get-product-inventory-list")]
    [Authorize]
    public async Task<IActionResult> GetProductInventoryList([FromQuery] int idPhysicalInventory)
    {
        // Obtener el ID del usuario del token JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Token de autorización inválido o usuario no encontrado.");
        }

        var result = await _physicalInventoryService.GetProductInventoryListAsync(idPhysicalInventory, userId);
        
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
    /// Obtener lista paginada de productos para conteo físico con filtros y catálogos
    /// Incluye paginación, filtros por texto, proveedor, familia, clase y línea,
    /// además de las listas de catálogos para filtros en el frontend
    /// </summary>
    /// <param name="idPhysicalInventory">ID del inventario físico</param>
    /// <param name="pageNumber">Número de página (por defecto: 1)</param>
    /// <param name="pageSize">Tamaño de página (por defecto: 50, máximo: 1000)</param>
    /// <param name="searchText">Filtro por descripción o nombre del producto</param>
    /// <param name="idSupplier">Filtro por ID del proveedor</param>
    /// <param name="familia">Filtro por familia (propiedad del producto)</param>
    /// <param name="clase">Filtro por clase (propiedad del producto)</param>
    /// <param name="linea">Filtro por línea (propiedad del producto)</param>
    /// <returns>Lista paginada de productos con catálogos para filtros</returns>
    [HttpGet("get-product-inventory-list-paginated")]
    [Authorize]
    public async Task<IActionResult> GetProductInventoryListPaginated(
        [FromQuery] int idPhysicalInventory,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchText = null,
        [FromQuery] int? idSupplier = null,
        [FromQuery] string? familia = null,
        [FromQuery] string? clase = null,
        [FromQuery] string? linea = null)
    {
        // Obtener el ID del usuario del token JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Token de autorización inválido o usuario no encontrado.");
        }

        // Validar parámetros de paginación
        if (pageNumber < 1)
        {
            return BadRequest("El número de página debe ser mayor a 0.");
        }

        if (pageSize < 1 || pageSize > 1000)
        {
            return BadRequest("El tamaño de página debe estar entre 1 y 1000.");
        }

        var pagination = new ProductInventoryListPaginationDto
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var filters = new ProductInventoryListFiltersDto
        {
            SearchText = searchText,
            IdSupplier = idSupplier,
            Familia = familia,
            Clase = clase,
            Linea = linea
        };

        var result = await _physicalInventoryService.GetProductInventoryListPaginatedAsync(
            idPhysicalInventory, userId, pagination, filters);
        
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
    /// Crear o actualizar un conteo específico de un producto en el inventario físico
    /// </summary>
    /// <param name="countDto">Datos del conteo a crear o actualizar</param>
    /// <returns>Detalle del conteo actualizado</returns>
    [HttpPost("physical-count")]
    public async Task<IActionResult> CreateOrUpdatePhysicalCount([FromBody] CreateOrUpdatePhysicalCountDto countDto)
    {
        var result = await _physicalInventoryService.CreateOrUpdatePhysicalCountAsync(countDto);
        
        if (result.Success)
        {
            return result.StatusCode == 201 ? Created(string.Empty, result) : Ok(result);
        }
        
        return result.StatusCode switch
        {
            400 => BadRequest(result),
            404 => NotFound(result),
            500 => StatusCode(500, result),
            _ => BadRequest(result)
        };
    }
}

