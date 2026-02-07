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
    [HttpPost("CreateProducts")]
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
    [HttpPost("CreateMultipleProducts")]
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
    /// <param name="mainName">Filtro por nombre del producto (opcional)</param>
    /// <param name="idSupplier">Filtro por ID del proveedor (opcional)</param>
    /// <param name="supplierName">Filtro por nombre del proveedor (opcional)</param>
    /// <param name="idProperty">Filtro por ID de propiedad: 1=Familia, 2=Clase, 3=Línea (opcional)</param>
    /// <param name="propertyName">Filtro por nombre de propiedad: Familia, Clase, Línea (opcional)</param>
    /// <param name="idPropertyValue">Filtro por ID de valor de propiedad (opcional)</param>
    /// <param name="propertyValue">Filtro por valor de propiedad: ACRILICOS, CORTE, etc (opcional)</param>
    [HttpGet("GetProduct")]
    public async Task<ActionResult> GetProductById(
        [FromQuery] int? id = null,
        [FromQuery] string? mainName = null,
        [FromQuery] int? idSupplier = null,
        [FromQuery] string? supplierName = null,
        [FromQuery] int? idProperty = null,
        [FromQuery] string? propertyName = null,
        [FromQuery] int? idPropertyValue = null,
        [FromQuery] string? propertyValue = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // Crear filtros opcionales
            ProductFilterDto? filters = null;
            if (!string.IsNullOrEmpty(mainName) || idSupplier.HasValue ||
                !string.IsNullOrEmpty(supplierName) || idProperty.HasValue ||
                !string.IsNullOrEmpty(propertyName) || idPropertyValue.HasValue ||
                !string.IsNullOrEmpty(propertyValue))
            {
                filters = new ProductFilterDto
                {
                    MainName = mainName,
                    IdSupplier = idSupplier,
                    SupplierName = supplierName,
                    IdProperty = idProperty,
                    PropertyName = propertyName,
                    IdPropertyValue = idPropertyValue,
                    PropertyValue = propertyValue
                };
            }

            // Si no se proporciona ID, devolver todos los productos (con filtros opcionales)
            if (!id.HasValue || id.Value == 0)
            {
                var pagination = new PaginationDto
                {
                    PageNumber = pageNumber > 0 ? pageNumber : 1,
                    PageSize = pageSize > 0 ? pageSize : 10
                };
                var products = await _productService.GetAllProductsAsync(filters, pagination);
                return Ok(new ApiResponse(products, "Productos obtenidos exitosamente", 200));
            }

            // Buscar producto por ID con filtros opcionales
            var product = await _productService.GetProductByIdAsync(id.Value, filters);

            if (product == null)
            {
                return NotFound(new ApiResponse($"Producto con ID {id} no encontrado o no cumple con los filtros", 404));
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
    [HttpPut("UpdateProduct/{id}")]
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
    [HttpDelete("DeleteProduct/{id}")]
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
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse("Error al eliminar el producto", 500));
        }
    }

    /// <summary>
    /// Importa productos desde un archivo Excel
    /// </summary>
    /// <param name="file">Archivo Excel que contiene los productos a importar</param>
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ImportProductResultDto>> ImportProducts(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No se proporcionó ningún archivo");

        if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
            return BadRequest("El archivo debe ser un Excel (.xlsx o .xls)");

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _productService.ImportProductsFromExcelAsync(stream);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al importar: {ex.Message}");
        }
    }
}