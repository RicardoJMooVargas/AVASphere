using AVASphere.ApplicationCore.Common.DTOs.ProductPropertiesDTOs;
using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Common.Interfaces;

namespace AVASphere.Infrastructure.Common.Services;

public class ProductPropertiesService : IProductPropertiesService
{
    private readonly IProductPropertiesRepository _repository;
    private readonly IProductRepository _productRepository;
    private readonly IPropertyValueRepository _propertyValueRepository;

    public ProductPropertiesService(
        IProductPropertiesRepository repository,
        IProductRepository productRepository,
        IPropertyValueRepository propertyValueRepository)
    {
        _repository = repository;
        _productRepository = productRepository;
        _propertyValueRepository = propertyValueRepository;
    }

    public async Task<ProductPropertiesResponseDto> CreateProductPropertiesAsync(CreateProductPropertiesDto dto)
    {
        // Validar que el producto existe
        var productExists = await _productRepository.ExistsAsync(dto.IdProduct);
        if (!productExists)
        {
            throw new KeyNotFoundException($"El producto con ID {dto.IdProduct} no existe.");
        }

        // Validar que el valor de propiedad existe
        var propertyValueExists = await _propertyValueRepository.ExistsAsync(dto.IdPropertyValue);
        if (!propertyValueExists)
        {
            throw new KeyNotFoundException($"El valor de propiedad con ID {dto.IdPropertyValue} no existe.");
        }

        var productProperties = new ProductProperties
        {
            CustomValue = dto.CustomValue,
            IdProduct = dto.IdProduct,
            IdPropertyValue = dto.IdPropertyValue
        };

        var created = await _repository.CreateProductPropertiesAsync(productProperties);

        // Recargar con includes
        var result = await _repository.GetByIdProductPropertiesAsync(created.IdProductProperties);
        return MapToResponseDto(result!);
    }

    public async Task<IEnumerable<ProductPropertiesResponseDto>> CreateMultipleProductPropertiesAsync(List<CreateProductPropertiesDto> dtos)
    {
        var results = new List<ProductPropertiesResponseDto>();

        foreach (var dto in dtos)
        {
            var created = await CreateProductPropertiesAsync(dto);
            results.Add(created);
        }

        return results;
    }

    public async Task<ProductPropertiesResponseDto> UpdateProductPropertiesAsync(int id, UpdateProductPropertiesDto dto)
    {
        var existing = await _repository.GetByIdProductPropertiesAsync(id);
        if (existing == null)
        {
            throw new KeyNotFoundException($"ProductProperties con ID {id} no encontrado");
        }

        // Validar que el producto existe
        var productExists = await _productRepository.ExistsAsync(dto.IdProduct);
        if (!productExists)
        {
            throw new KeyNotFoundException($"El producto con ID {dto.IdProduct} no existe.");
        }

        // Validar que el valor de propiedad existe
        var propertyValueExists = await _propertyValueRepository.ExistsAsync(dto.IdPropertyValue);
        if (!propertyValueExists)
        {
            throw new KeyNotFoundException($"El valor de propiedad con ID {dto.IdPropertyValue} no existe.");
        }

        existing.CustomValue = dto.CustomValue;
        existing.IdProduct = dto.IdProduct;
        existing.IdPropertyValue = dto.IdPropertyValue;

        var updated = await _repository.UpdateProductPropertiesAsync(existing);

        // Recargar con includes
        var result = await _repository.GetByIdProductPropertiesAsync(updated.IdProductProperties);
        return MapToResponseDto(result!);
    }

    public async Task<bool> DeleteProductPropertiesAsync(int id)
    {
        return await _repository.DeleteProductPropertiesAsync(id);
    }

    public async Task<ProductPropertiesResponseDto?> GetByIdProductPropertiesAsync(int id)
    {
        var productProperties = await _repository.GetByIdProductPropertiesAsync(id);
        if (productProperties == null) return null;

        return MapToResponseDto(productProperties);
    }

    private static ProductPropertiesResponseDto MapToResponseDto(ProductProperties productProperties)
    {
        return new ProductPropertiesResponseDto
        {
            IdProductProperties = productProperties.IdProductProperties,
            CustomValue = productProperties.CustomValue,
            IdProduct = productProperties.IdProduct,
            IdPropertyValue = productProperties.IdPropertyValue,
            ProductName = productProperties.Product?.MainName,
            PropertyValueName = productProperties.PropertyValue?.Value
        };
    }

}