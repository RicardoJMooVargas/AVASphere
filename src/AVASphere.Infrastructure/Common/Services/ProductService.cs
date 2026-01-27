using AVASphere.ApplicationCore.Common.DTOs.ProductDTOs;
using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AVASphere.Infrastructure.Common.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductResponseDto> CreateProductAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            MainName = dto.MainName,
            Unit = dto.Unit,
            Description = dto.Description,
            Quantity = dto.Quantity,
            Taxes = dto.Taxes,
            IdSupplier = dto.IdSupplier,
            CodeJson = dto.CodeJson ?? new(),
            CostsJson = dto.CostsJson ?? new(),
            CategoriesJsons = dto.CategoriesJsons ?? new(),
            SolutionsJsons = dto.SolutionsJsons ?? new()
        };

        var createdProduct = await _productRepository.CreateProductsAsync(product);
        return MapToResponseDto(createdProduct);
    }

    public async Task<IEnumerable<ProductResponseDto>> CreateMultipleProductsAsync(List<CreateProductDto> createProductDtos)
    {
        var createdProducts = new List<ProductResponseDto>();

        foreach (var dto in createProductDtos)
        {
            var product = new Product
            {
                MainName = dto.MainName,
                Unit = dto.Unit,
                Description = dto.Description,
                Quantity = dto.Quantity,
                Taxes = dto.Taxes,
                IdSupplier = dto.IdSupplier,
                CodeJson = dto.CodeJson ?? new(),
                CostsJson = dto.CostsJson ?? new(),
                CategoriesJsons = dto.CategoriesJsons ?? new(),
                SolutionsJsons = dto.SolutionsJsons ?? new()
            };

            var createdProduct = await _productRepository.CreateProductsAsync(product);
            createdProducts.Add(MapToResponseDto(createdProduct));
        }

        return createdProducts;
    }

    public async Task<ProductResponseDto> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var existingProduct = await _productRepository.GetByIdProductsAsync(id);
        if (existingProduct == null)
        {
            throw new KeyNotFoundException($"Product con ID {id} no encontrado.");
        }

        existingProduct.MainName = dto.MainName;
        existingProduct.Unit = dto.Unit;
        existingProduct.Description = dto.Description;
        existingProduct.Quantity = dto.Quantity;
        existingProduct.Taxes = dto.Taxes;
        existingProduct.IdSupplier = dto.IdSupplier;
        existingProduct.CodeJson = dto.CodeJson ?? new();
        existingProduct.CostsJson = dto.CostsJson ?? new();
        existingProduct.CategoriesJsons = dto.CategoriesJsons ?? new();
        existingProduct.SolutionsJsons = dto.SolutionsJsons ?? new();


        var updatedProduct = await _productRepository.UpdateProductsAsync(existingProduct);
        return MapToResponseDto(updatedProduct);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        return await _productRepository.DeleteProductsAsync(id);
    }

    public async Task<ProductResponseDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdProductsAsync(id);
        if (product == null)
        {
            return null;
        }

        return MapToResponseDto(product);
    }

    public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllProductsAsync();
        return products.Select(MapToResponseDto);
    }

    private static ProductResponseDto MapToResponseDto(Product product)
    {
        return new ProductResponseDto
        {
            IdProduct = product.IdProduct,
            MainName = product.MainName,
            Unit = product.Unit,
            Description = product.Description,
            Quantity = product.Quantity,
            Taxes = product.Taxes,
            IdSupplier = product.IdSupplier,
            CodeJson = product.CodeJson?.ToList() ?? new(),
            CostsJson = product.CostsJson?.ToList() ?? new(),
            CategoriesJsons = product.CategoriesJsons?.ToList() ?? new(),
            SolutionsJsons = product.SolutionsJsons?.ToList() ?? new()
        };
    }
}