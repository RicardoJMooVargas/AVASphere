using AVASphere.ApplicationCore.Common.DTOs.ProductDTOs;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IProductService
{
    Task<ProductResponseDto> CreateProductAsync(CreateProductDto createProductDto);
    Task<IEnumerable<ProductResponseDto>> CreateMultipleProductsAsync(List<CreateProductDto> createProductDtos);
    Task<ProductResponseDto> UpdateProductAsync(int idProduct, UpdateProductDto updateProductDto);
    Task<bool> DeleteProductAsync(int idProduct);
    Task<ProductResponseDto?> GetProductByIdAsync(int idProduct, ProductFilterDto? filters = null);
    Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(ProductFilterDto? filters = null);
    Task<ImportProductResultDto> ImportProductsFromExcelAsync(Stream excelStream);
}