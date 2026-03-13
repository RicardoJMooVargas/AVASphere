using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Enums;
using AVASphere.ApplicationCore.Inventory.DTOs;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AVASphere.WebApi.Common.Controllers;

[ApiController]
[Route("api/common/[controller]")]
[ApiExplorerSettings(GroupName = nameof(SystemModule.Common))]
[Tags("Common - Inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// <summary>
    /// Importa inventario desde un archivo Excel
    /// </summary>
    /// <param name="file">Archivo Excel con el inventario</param>
    [HttpPost("ImportFromExcel")]
    public async Task<ActionResult> ImportInventoryFromExcel(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse("No se proporcionó ningún archivo", 400));
            }

            // Validar que sea un archivo Excel
            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new ApiResponse("El archivo debe ser un archivo Excel (.xlsx o .xls)", 400));
            }

            using (var stream = file.OpenReadStream())
            {
                var result = await _inventoryService.ImportInventoryFromExcelAsync(stream);

                // Construir mensaje de respuesta
                var message = $"Importación completada. Total: {result.TotalRows} filas, " +
                             $"Exitosas: {result.SuccessfulImports}, " +
                             $"Fallidas: {result.FailedImports}";

                if (result.ProductsNotFound > 0)
                    message += $", Productos no encontrados: {result.ProductsNotFound}";

                if (result.WarehousesNotFound > 0)
                    message += $", Bodegas no encontradas: {result.WarehousesNotFound}";

                return Ok(new ApiResponse(result, message, 200));
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse($"Error al importar el inventario: {ex.Message}", 500));
        }
    }

    /// <summary>
    /// Importa inventario de ubicación desde un archivo Excel
    /// </summary>
    /// <param name="file">Archivo Excel con el inventario de ubicación</param>
    [HttpPost("ImportInventoryUbication")]
    public async Task<ActionResult> ImportInventoryUbication(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse("No se proporcionó ningún archivo", 400));
            }

            // Validar que sea un archivo Excel
            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new ApiResponse("El archivo debe ser un archivo Excel (.xlsx o .xls)", 400));
            }

            using (var stream = file.OpenReadStream())
            {
                var result = await _inventoryService.ImportInventoryUbicationFromExcelAsync(stream);

                // Construir mensaje de respuesta
                var message = $"Importación completada. Total: {result.TotalRows} filas, " +
                             $"Exitosas: {result.SuccessfulImports}, " +
                             $"Fallidas: {result.FailedImports}";

                if (result.ProductsNotFound > 0)
                    message += $", Productos no encontrados: {result.ProductsNotFound}";

                if (result.WarehousesNotFound > 0)
                    message += $", Bodegas no encontradas: {result.WarehousesNotFound}";

                return Ok(new ApiResponse(result, message, 200));
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse($"Error al importar el inventario de ubicación: {ex.Message}", 500));
        }
    }

    /// <summary>
    /// Crea un nuevo registro de inventario
    /// </summary>
    /// <param name="createDto">Datos del inventario</param>
    [HttpPost("Create")]
    public async Task<ActionResult> CreateInventory([FromBody] CreateInventoryDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse("Datos de entrada inválidos", 400, ModelState));
            }

            var inventory = await _inventoryService.CreateInventoryAsync(createDto);

            return Ok(new ApiResponse(inventory, "Inventario creado exitosamente", 201));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(ex.Message, 404));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse($"Error al crear el inventario: {ex.Message}", 500));
        }
    }

    /// <summary>
    /// Obtiene todo el inventario con filtros opcionales y paginación
    /// </summary>
    /// <param name="pageNumber">Número de página (base 1, por defecto 1)</param>
    /// <param name="pageSize">Tamaño de página (por defecto 20, máximo 10000)</param>
    /// <param name="idInventory">Filtrar por ID de inventario (opcional)</param>
    /// <param name="idWarehouse">Filtrar por ID de bodega (opcional)</param>
    /// <param name="warehouseCode">Filtrar por código de bodega: AVA01, AVA02, AVA03, AVA04 (opcional)</param>
    /// <param name="idProduct">Filtrar por ID de producto (opcional)</param>
    /// <param name="productName">Buscar por nombre/descripción del producto (opcional)</param>
    [HttpGet("GetAllInventory")]
    public async Task<ActionResult> GetAllInventory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? idInventory = null,
        [FromQuery] int? idWarehouse = null,
        [FromQuery] string? warehouseCode = null,
        [FromQuery] int? idProduct = null,
        [FromQuery] string? productName = null)
    {
        try
        {
            var paginatedResult = await _inventoryService.GetAllInventoryAsync(
                pageNumber,
                pageSize,
                idInventory,
                idWarehouse,
                warehouseCode,
                idProduct,
                productName);

            return Ok(new ApiResponse(paginatedResult, "Inventarios obtenidos exitosamente", 200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse($"Error al obtener los inventarios: {ex.Message}", 500));
        }
    }
}
