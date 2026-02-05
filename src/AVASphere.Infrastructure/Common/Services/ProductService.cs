using AVASphere.ApplicationCore.Common.DTOs.ProductDTOs;
using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Projects.Entities.jsons;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;

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
    public async Task<ProductResponseDto?> GetProductByIdAsync(int id, ProductFilterDto? filters = null)
    {
        var product = await _productRepository.GetByIdProductsAsync(id, filters);

        if (product == null)
            return null;

        return MapToResponseDto(product);
    }
    public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(ProductFilterDto? filters = null)
    {
        var products = await _productRepository.GetAllProductsAsync(filters);
        return products.Select(MapToResponseDto);
    }
    private ProductResponseDto MapToResponseDto(Product product)
    {
        return new ProductResponseDto
        {
            IdProduct = product.IdProduct,
            MainName = product.MainName,
            SupplierName = product.Supplier?.Name ?? "",
            Unit = product.Unit,
            Description = product.Description,
            Quantity = product.Quantity,
            Taxes = product.Taxes,
            IdSupplier = product.IdSupplier,
            CodeJson = product.CodeJson?.ToList() ?? new(),
            CostsJson = product.CostsJson?.ToList() ?? new(),
            CategoriesJsons = product.CategoriesJsons?.ToList() ?? new(),
            SolutionsJsons = product.SolutionsJsons?.ToList() ?? new(),

            // Mapear ProductProperties con nombres de PropertyValue y Property
            ProductProperties = product.ProductProperties?.Select(pp => new ProductPropertyDto
            {
                IdProductProperties = pp.IdProductProperties,
                CustomValue = pp.CustomValue ?? "",
                IdProduct = pp.IdProduct,
                IdPropertyValue = pp.IdPropertyValue,
                PropertyValueName = pp.PropertyValue?.Value ?? "",
                PropertyName = pp.PropertyValue?.Property?.Name ?? ""
            }).ToList() ?? new List<ProductPropertyDto>()
        };
    }

    public async Task<ImportProductResultDto> ImportProductsFromExcelAsync(Stream excelStream)
    {
        var result = new ImportProductResultDto();

        using (var workbook = new XLWorkbook(excelStream))
        {
            var worksheet = workbook.Worksheet(1);
            var rowCount = worksheet.LastRowUsed().RowNumber();

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    // Columna D: Proveedor
                    var supplierName = worksheet.Cell(row, 4).GetValue<string>().Trim();

                    // Buscar proveedor por nombre
                    var supplier = await _productRepository.GetSupplierByNameAsync(supplierName);

                    if (supplier == null)
                    {
                        result.Errors.Add($"Fila {row}: Proveedor '{supplierName}' no encontrado");
                        result.FailedImports++;
                        continue;
                    }

                    // Columna A: Código
                    var code = worksheet.Cell(row, 1).GetValue<string>().Trim();

                    // Columna B: Descripción
                    var description = worksheet.Cell(row, 2).GetValue<string>().Trim();

                    // Columna C: Unidad
                    var unit = worksheet.Cell(row, 3).GetValue<string>().Trim();

                    var createDto = new CreateProductDto
                    {
                        MainName = description,
                        SupplierName = supplierName,
                        Unit = unit,
                        Description = description,
                        Quantity = 0,
                        Taxes = 16,
                        IdSupplier = supplier.IdSupplier,
                        CodeJson = new List<CodeJson>
                        {
                            new CodeJson
                            {
                                Index = 0,
                                Type = "Principal",
                                Code = code
                            }
                        },
                        CostsJson = new List<CostsJson>(),
                        CategoriesJsons = new List<CategoriesJson>(),
                        SolutionsJsons = new List<SolutionsJson>()
                    };

                    var productResponse = await CreateProductAsync(createDto);

                    // Columna E: Familia
                    var familia = worksheet.Cell(row, 5).GetValue<string>().Trim();
                    if (!string.IsNullOrWhiteSpace(familia))
                    {
                        try
                        {
                            var familiaId = await _productRepository.FindPropertyValueIdAsync("Familia", familia);
                            if (familiaId.HasValue)
                            {
                                await _productRepository.CreateProductPropertyAsync(productResponse.IdProduct, familiaId.Value);
                            }
                            else
                            {
                                result.Errors.Add($"Fila {row}: PropertyValue 'Familia' con valor '{familia}' no encontrado");
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Fila {row}: Error al crear ProductProperty Familia - {ex.Message}");
                        }
                    }

                    // Columna F: Clase
                    var clase = worksheet.Cell(row, 6).GetValue<string>().Trim();
                    if (!string.IsNullOrWhiteSpace(clase))
                    {
                        try
                        {
                            var claseId = await _productRepository.FindPropertyValueIdAsync("Clase", clase);
                            if (claseId.HasValue)
                            {
                                await _productRepository.CreateProductPropertyAsync(productResponse.IdProduct, claseId.Value);
                            }
                            else
                            {
                                result.Errors.Add($"Fila {row}: PropertyValue 'Clase' con valor '{clase}' no encontrado");
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Fila {row}: Error al crear ProductProperty Clase - {ex.Message}");
                        }
                    }

                    // Columna G: Línea
                    var linea = worksheet.Cell(row, 7).GetValue<string>().Trim();
                    if (!string.IsNullOrWhiteSpace(linea))
                    {
                        try
                        {
                            var lineaId = await _productRepository.FindPropertyValueIdAsync("Línea", linea);
                            if (lineaId.HasValue)
                            {
                                await _productRepository.CreateProductPropertyAsync(productResponse.IdProduct, lineaId.Value);
                            }
                            else
                            {
                                result.Errors.Add($"Fila {row}: PropertyValue 'Línea' con valor '{linea}' no encontrado");
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Fila {row}: Error al crear ProductProperty Línea - {ex.Message}");
                        }
                    }

                    result.SuccessfulImports++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Fila {row}: {ex.Message}");
                    result.FailedImports++;
                }
            }

            result.TotalRows = rowCount - 1;
        }

        return result;
    }


}