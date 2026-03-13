using AVASphere.ApplicationCore.Common.DTOs.ProductDTOs;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IProductService
{
    Task<ProductResponseDto> CreateProductAsync(CreateProductDto createProductDto);
    Task<IEnumerable<ProductResponseDto>> CreateMultipleProductsAsync(List<CreateProductDto> createProductDtos);
    Task<ProductResponseDto> UpdateProductAsync(int idProduct, UpdateProductDto updateProductDto);
    Task<bool> DeleteProductAsync(int idProduct);
    Task<ProductResponseDto?> GetProductByIdAsync(int idProduct, ProductFilterDto? filters = null);
    Task<bool> AddProductImageAsync(int idProduct, string imageUrl);
    Task<bool> RemoveProductImageAsync(int idProduct, string imageUrl);

    /// <summary>
    /// Obtiene todos los productos con filtros y paginación
    /// </summary>
    /// <param name="pageNumber">Número de página (base 1, por defecto 1)</param>
    /// <param name="pageSize">Tamaño de página (por defecto 20, máximo 10000)</param>
    /// <param name="filters">Filtros opcionales</param>
    /// <returns>Respuesta paginada con productos y metadatos de paginación</returns>
    Task<PaginatedProductResponseDto> GetAllProductsAsync(
        int pageNumber = 1,
        int pageSize = 20,
        ProductFilterDto? filters = null);

    Task<ImportProductResultDto> ImportProductsFromExcelAsync(Stream excelStream);
}