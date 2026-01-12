using AVASphere.ApplicationCore.Common.DTOs.ProductDTOs;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IProductService
{
    Task<ProductResponseDto> CreateProductAsync(CreateProductDto createProductDto);
    Task<ProductResponseDto> UpdateProductAsync(int idProduct, UpdateProductDto updateProductDto);
    Task<bool> DeleteProductAsync(int idProduct);
    Task<ProductResponseDto?> GetProductByIdAsync(int idProduct);
    Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync();
}