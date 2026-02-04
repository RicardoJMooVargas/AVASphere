using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.DTOs.ProductDTOs;
using AVASphere.ApplicationCore.Common.Enums;
using AVASphere.ApplicationCore.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AVASphere.WebApi.Common.Controllers;

[ApiController]
[Route("api/common/[controller]")]
[ApiExplorerSettings(GroupName = nameof(SystemModule.Common))]
[Tags("Common - Product")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Crea un nuevo producto
    /// </summary>
    /// <param name="createProductDto">Datos del producto a crear</param>
    [HttpPost("create-products")]
    public async Task<ActionResult> CreateProducts(CreateProductDto createProductDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse("Datos de entrada inválidos", 400, ModelState));
            }

            var product = await _productService.CreateProductAsync(createProductDto);

            return CreatedAtAction(
                nameof(GetProductById),
                new { id = product.IdProduct },
                new ApiResponse(product, "Producto creado exitosamente", 201));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(ex.Message, 404));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse($"Error al crear el producto: {ex.Message}", 500));
        }
    }

    /// <summary>
    /// Crea múltiples productos de una sola vez
    /// </summary>
    /// <param name="createProductDtos">Lista de productos a crear</param>
    [HttpPost("create-multiple-products")]
    public async Task<ActionResult> CreateMultipleProducts([FromBody] List<CreateProductDto> createProductDtos)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse("Datos de entrada inválidos", 400, ModelState));
            }

            if (createProductDtos == null || !createProductDtos.Any())
            {
                return BadRequest(new ApiResponse("Debe proporcionar al menos un producto", 400));
            }

            var products = await _productService.CreateMultipleProductsAsync(createProductDtos);

            return Ok(new ApiResponse(products, $"{products.Count()} productos creados exitosamente", 201));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(ex.Message, 404));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse($"Error al crear los productos: {ex.Message}", 500));
        }
    }

    /// <summary>
    /// Obtiene un producto por su ID o todos los productos si no se especifica ID
    /// </summary>
    /// <param name="id">ID del producto (opcional). Si no se proporciona, devuelve todos los productos</param>
    [HttpGet("get-product-by-id")]
    public async Task<ActionResult> GetProductById([FromQuery] int? id = null)
    {
        try
        {
            // Si no se proporciona ID o es 0, devolver todos los productos
            if (!id.HasValue || id.Value == 0)
            {
                var products = await _productService.GetAllProductsAsync();
                return Ok(new ApiResponse(products, "Productos obtenidos exitosamente", 200));
            }

            // Si se proporciona un ID, devolver ese producto específico
            var product = await _productService.GetProductByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound(new ApiResponse($"Producto con ID {id} no encontrado", 404));
            }

            return Ok(new ApiResponse(product, "Producto obtenido exitosamente", 200));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse($"Producto con ID {id} no encontrado", 404));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse($"Error al obtener el producto: {ex.Message} - InnerException: {ex.InnerException?.Message}", 500));
        }
    }

    /// <summary>
    /// Actualiza un producto existente
    /// </summary>
    /// <param name="id">ID del producto a actualizar</param>
    /// <param name="updateProductDto">Datos actualizados del producto</param>
    [HttpPut("update-product/{id}")]
    public async Task<ActionResult> UpdateProduct(int id, UpdateProductDto updateProductDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse("Datos de entrada inválidos", 400, ModelState));
            }

            var updatedProduct = await _productService.UpdateProductAsync(id, updateProductDto);

            return Ok(new ApiResponse(updatedProduct, "Producto actualizado exitosamente", 200));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse($"Producto con ID {id} no encontrado", 404));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse($"Error al actualizar el producto: {ex.Message} - InnerException: {ex.InnerException?.Message}", 500));
        }
    }

    /// <summary>
    /// Elimina un producto
    /// </summary>
    /// <param name="id">ID del producto a eliminar</param>
    [HttpDelete("delete-product/{id}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        try
        {
            var deleted = await _productService.DeleteProductAsync(id);

            if (!deleted)
            {
                return NotFound(new ApiResponse($"Producto con ID {id} no encontrado", 404));
            }

            return Ok(new ApiResponse(null, $"Producto con ID {id} eliminado exitosamente", 200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse("Error al eliminar el producto", 500));
        }
    }
}