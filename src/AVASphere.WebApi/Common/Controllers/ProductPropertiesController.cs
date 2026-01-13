using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.DTOs.ProductPropertiesDTOs;
using AVASphere.ApplicationCore.Common.Enums;
using AVASphere.ApplicationCore.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AVASphere.WebApi.Common.Controllers;

[ApiController]
[Route("api/common/[controller]")]
[ApiExplorerSettings(GroupName = nameof(SystemModule.Common))]
[Tags("Common - ProductProperties")]
public class ProductPropertiesController : ControllerBase
{
    private readonly IProductPropertiesService _productPropertiesService;

    public ProductPropertiesController(IProductPropertiesService productPropertiesService)
    {
        _productPropertiesService = productPropertiesService;
    }

    /// <summary>
    /// Crea una nueva propiedad de producto
    /// </summary>
    [HttpPost("create-product-properties")]
    public async Task<ActionResult> CreateProductProperties([FromBody] CreateProductPropertiesDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse("Datos de entrada inválidos", 400, ModelState));
            }

            var productProperties = await _productPropertiesService.CreateProductPropertiesAsync(dto);

            return CreatedAtAction(
                nameof(GetByIdProductProperties),
                new { id = productProperties.IdProductProperties },
                new ApiResponse(productProperties, "Propiedad de producto creada exitosamente", 201));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(ex.Message, 404));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse($"Error al crear la propiedad de producto: {ex.Message}", 500));
        }
    }

    /// <summary>
    /// Crea múltiples propiedades de producto
    /// </summary>
    [HttpPost("create-multiple-product-properties")]
    public async Task<ActionResult> CreateMultipleProductProperties([FromBody] List<CreateProductPropertiesDto> dtos)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse("Datos de entrada inválidos", 400, ModelState));
            }

            var productProperties = await _productPropertiesService.CreateMultipleProductPropertiesAsync(dtos);

            return Ok(new ApiResponse(productProperties, "Propiedades de producto creadas exitosamente", 201));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(ex.Message, 404));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse($"Error al crear las propiedades de producto: {ex.Message}", 500));
        }
    }

    /// <summary>
    /// Actualiza una propiedad de producto
    /// </summary>
    [HttpPut("update-product-properties/{id}")]
    public async Task<ActionResult> UpdateProductProperties(int id, [FromBody] UpdateProductPropertiesDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse("Datos de entrada inválidos", 400, ModelState));
            }

            var updated = await _productPropertiesService.UpdateProductPropertiesAsync(id, dto);

            return Ok(new ApiResponse(updated, "Propiedad de producto actualizada exitosamente", 200));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(ex.Message, 404));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse($"Error al actualizar la propiedad de producto: {ex.Message}", 500));
        }
    }

    /// <summary>
    /// Elimina una propiedad de producto
    /// </summary>
    [HttpDelete("delete-product-properties/{id}")]
    public async Task<ActionResult> DeleteProductProperties(int id)
    {
        try
        {
            var deleted = await _productPropertiesService.DeleteProductPropertiesAsync(id);

            if (!deleted)
            {
                return NotFound(new ApiResponse($"Propiedad de producto con ID {id} no encontrada", 404));
            }

            return Ok(new ApiResponse(null, $"Propiedad de producto con ID {id} eliminada exitosamente", 200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse($"Error al eliminar la propiedad de producto: {ex.Message}", 500));
        }
    }

    /// <summary>
    /// Obtiene una propiedad de producto por ID
    /// </summary>
    [HttpGet("get-product-properties-by-id")]
    public async Task<ActionResult> GetByIdProductProperties([FromQuery] int? id = null)
    {
        try
        {
            if (!id.HasValue || id.Value == 0)
            {
                return BadRequest(new ApiResponse("Debe proporcionar un ID válido", 400));
            }

            var productProperties = await _productPropertiesService.GetByIdProductPropertiesAsync(id.Value);
            if (productProperties == null)
            {
                return NotFound(new ApiResponse($"Propiedad de producto con ID {id} no encontrada", 404));
            }

            return Ok(new ApiResponse(productProperties, "Propiedad de producto obtenida exitosamente", 200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse($"Error al obtener la propiedad de producto: {ex.Message}", 500));
        }
    }
}