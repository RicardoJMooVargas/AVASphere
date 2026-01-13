using AVASphere.ApplicationCore.Common.DTOs.ProductPropertiesDTOs;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IProductPropertiesService
{
    Task<ProductPropertiesResponseDto> CreateProductPropertiesAsync(CreateProductPropertiesDto dto);
    Task<IEnumerable<ProductPropertiesResponseDto>> CreateMultipleProductPropertiesAsync(List<CreateProductPropertiesDto> dtos);
    Task<ProductPropertiesResponseDto> UpdateProductPropertiesAsync(int id, UpdateProductPropertiesDto dto);
    Task<bool> DeleteProductPropertiesAsync(int id);
    Task<ProductPropertiesResponseDto?> GetByIdProductPropertiesAsync(int id);
}