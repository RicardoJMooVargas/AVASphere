using Microsoft.AspNetCore.Mvc;
using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Inventory.DTOs;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace AVASphere.WebApi.Inventory.Controllers;

[ApiController]
[Route("api/inventory/[controller]")]
[ApiExplorerSettings(GroupName = "Inventory")]
[Tags("Inventory - Physical Inventory Details")]
public class PhysicalInventoryDetailController : ControllerBase
{
    private readonly IPhysicalInventoryDetailService _physicalInventoryDetailService;
    private readonly ILogger<PhysicalInventoryDetailController> _logger;

    public PhysicalInventoryDetailController(
        IPhysicalInventoryDetailService physicalInventoryDetailService,
        ILogger<PhysicalInventoryDetailController> logger)
    {
        _physicalInventoryDetailService = physicalInventoryDetailService;
        _logger = logger;
    }

    /// <summary>
    /// Actualiza la cantidad física de un detalle específico
    /// </summary>
    [HttpPut("update-physical-quantity")]
    public async Task<IActionResult> UpdatePhysicalQuantity([FromBody] PhysicalQuantityUpdateDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Data = ModelState,
                    StatusCode = 400
                });
            }

            var result = await _physicalInventoryDetailService.UpdatePhysicalQuantityAsync(updateDto);

            if (result == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Detalle de inventario físico no encontrado",
                    StatusCode = 404
                });
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Cantidad física actualizada exitosamente",
                Data = result,
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cantidad física");
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "Error interno del servidor al actualizar cantidad física",
                StatusCode = 500
            });
        }
    }

    /// <summary>
    /// Crea un nuevo registro de detalle de inventario físico con cantidad física
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreatePhysicalInventoryDetail([FromBody] PhysicalInventoryDetailCreateDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Data = ModelState,
                    StatusCode = 400
                });
            }

            var result = await _physicalInventoryDetailService.CreatePhysicalInventoryDetailAsync(createDto);

            return CreatedAtAction(
                nameof(GetPhysicalInventoryDetailById),
                new { id = result.IdPhysicalInventoryDetail },
                new ApiResponse
                {
                    Success = true,
                    Message = "Detalle de inventario físico creado exitosamente",
                    Data = result,
                    StatusCode = 201
                });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Error de validación al crear detalle de inventario físico");
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = ex.Message,
                StatusCode = 400
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear detalle de inventario físico");
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "Error interno del servidor al crear detalle de inventario físico",
                StatusCode = 500
            });
        }
    }

    /// <summary>
    /// Obtiene un detalle de inventario físico por su ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPhysicalInventoryDetailById([FromRoute] int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "ID inválido",
                    StatusCode = 400
                });
            }

            var result = await _physicalInventoryDetailService.GetPhysicalInventoryDetailByIdAsync(id);

            if (result == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Detalle de inventario físico no encontrado",
                    StatusCode = 404
                });
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Detalle de inventario físico obtenido exitosamente",
                Data = result,
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de inventario físico con ID: {Id}", id);
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "Error interno del servidor al obtener detalle de inventario físico",
                StatusCode = 500
            });
        }
    }

    /// <summary>
    /// Obtiene todos los detalles de un inventario físico específico
    /// </summary>
    [HttpGet("by-inventory/{idPhysicalInventory:int}")]
    public async Task<IActionResult> GetPhysicalInventoryDetailsByInventoryId([FromRoute] int idPhysicalInventory)
    {
        try
        {
            if (idPhysicalInventory <= 0)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "ID de inventario físico inválido",
                    StatusCode = 400
                });
            }

            var results = await _physicalInventoryDetailService.GetPhysicalInventoryDetailsByInventoryIdAsync(idPhysicalInventory);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Detalles de inventario físico obtenidos exitosamente",
                Data = results,
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalles de inventario físico para inventario ID: {InventoryId}", idPhysicalInventory);
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "Error interno del servidor al obtener detalles de inventario físico",
                StatusCode = 500
            });
        }
    }

    /// <summary>
    /// Recalcula automáticamente todas las diferencias para un inventario físico específico
    /// </summary>
    [HttpPost("recalculate-differences/{idPhysicalInventory:int}")]
    public async Task<IActionResult> RecalculateDifferences([FromRoute] int idPhysicalInventory)
    {
        try
        {
            if (idPhysicalInventory <= 0)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "ID de inventario físico inválido",
                    StatusCode = 400
                });
            }

            var result = await _physicalInventoryDetailService.RecalculateDifferencesAsync(idPhysicalInventory);

            if (!result)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "No se encontraron detalles para recalcular o inventario físico no encontrado",
                    StatusCode = 404
                });
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Diferencias recalculadas exitosamente",
                Data = new { IdPhysicalInventory = idPhysicalInventory },
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al recalcular diferencias para inventario físico ID: {InventoryId}", idPhysicalInventory);
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "Error interno del servidor al recalcular diferencias",
                StatusCode = 500
            });
        }
    }

    /// <summary>
    /// Endpoint simplificado para solo actualizar la cantidad física (para contabilización rápida)
    /// </summary>
    [HttpPut("quick-count")]
    public async Task<IActionResult> QuickPhysicalCount([FromBody] QuickPhysicalCountDto quickCountDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Data = ModelState,
                    StatusCode = 400
                });
            }

            var updateDto = new PhysicalQuantityUpdateDto
            {
                IdPhysicalInventoryDetail = quickCountDto.IdPhysicalInventoryDetail,
                PhysicalQuantity = quickCountDto.PhysicalQuantity,
                Observations = quickCountDto.Observations
            };

            var result = await _physicalInventoryDetailService.UpdatePhysicalQuantityAsync(updateDto);

            if (result == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Detalle de inventario físico no encontrado",
                    StatusCode = 404
                });
            }

            // Respuesta simplificada para conteo rápido
            var quickResponse = new
            {
                result.IdPhysicalInventoryDetail,
                result.PhysicalQuantity,
                result.SystemQuantity,
                result.Difference,
                result.ProductName,
                result.ProductCode,
                Status = result.Difference == 0 ? "Exacto" : result.Difference > 0 ? "Sobrante" : "Faltante"
            };

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Conteo físico actualizado exitosamente",
                Data = quickResponse,
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en conteo físico rápido");
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "Error interno del servidor en conteo físico",
                StatusCode = 500
            });
        }
    }
}

/// <summary>
/// DTO simplificado para conteo rápido
/// </summary>
public class QuickPhysicalCountDto
{
    [Required(ErrorMessage = "El ID del detalle es requerido")]
    public int IdPhysicalInventoryDetail { get; set; }
    
    [Required(ErrorMessage = "La cantidad física es requerida")]
    [Range(0, double.MaxValue, ErrorMessage = "La cantidad física debe ser mayor o igual a 0")]
    public double PhysicalQuantity { get; set; }
    
    public string? Observations { get; set; }
}