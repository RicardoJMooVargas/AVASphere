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

        // Solo actualizar los campos que se envían (no nulos)
        if (dto.MainName != null)
            existingProduct.MainName = dto.MainName;

        if (dto.Unit != null)
            existingProduct.Unit = dto.Unit;

        if (dto.Description != null)
            existingProduct.Description = dto.Description;

        if (dto.Quantity.HasValue)
            existingProduct.Quantity = dto.Quantity.Value;

        if (dto.Taxes.HasValue)
            existingProduct.Taxes = dto.Taxes.Value;

        if (dto.IdSupplier.HasValue)
            existingProduct.IdSupplier = dto.IdSupplier.Value;

        if (dto.CodeJson != null)
            existingProduct.CodeJson = dto.CodeJson;

        if (dto.CostsJson != null)
            existingProduct.CostsJson = dto.CostsJson;

        if (dto.CategoriesJsons != null)
            existingProduct.CategoriesJsons = dto.CategoriesJsons;

        if (dto.SolutionsJsons != null)
            existingProduct.SolutionsJsons = dto.SolutionsJsons;

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
    /// <summary>
    /// Obtiene todos los productos con filtros y paginación (OPTIMIZADO)
    /// </summary>
    public async Task<PaginatedProductResponseDto> GetAllProductsAsync(
        int pageNumber = 1,
        int pageSize = 20,
        ProductFilterDto? filters = null)
    {
        // Validar parámetros de paginación
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 10000) pageSize = 10000; // Límite máximo de 10000 registros por página

        // Optimización 1: Obtener el total de registros SIN cargar los datos
        var totalCount = await _productRepository.GetProductCountAsync(filters);

        // Optimización 2: Calcular páginas antes de cargar datos
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Optimización 3: Aplicar paginación en la base de datos (no en memoria)
        var pagination = new PaginationDto
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        // Optimización 4: Solo cargar los productos de la página actual
        var pagedProducts = await _productRepository.GetAllProductsAsync(filters, pagination);

        // Mapear a DTOs
        var productDtos = pagedProducts.Select(MapToResponseDto).ToList();

        // Crear respuesta paginada
        return new PaginatedProductResponseDto
        {
            Items = productDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = pageNumber > 1,
            HasNextPage = pageNumber < totalPages
        };
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

    /// <summary>
    /// Importa productos desde un archivo Excel. El archivo debe tener las siguientes columnas:
    /// </summary>
    public async Task<ImportProductResultDto> ImportProductsFromExcelAsync(Stream excelStream)
    {
        var result = new ImportProductResultDto();

        // PRE-CARGAR TODOS LOS DATOS NECESARIOS ANTES DEL BUCLE
        var suppliersDict = await _productRepository.GetAllSuppliersAsync();
        var familiaValuesDict = await _productRepository.GetPropertyValueIdsByPropertyNameAsync("Familia");
        var claseValuesDict = await _productRepository.GetPropertyValueIdsByPropertyNameAsync("Clase");
        var lineaValuesDict = await _productRepository.GetPropertyValueIdsByPropertyNameAsync("Línea");

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

                    // Buscar proveedor en el diccionario precargado
                    if (!suppliersDict.TryGetValue(supplierName.ToLower(), out var supplier))
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

                    // Columna E: Familia - buscar en diccionario precargado
                    var familia = worksheet.Cell(row, 5).GetValue<string>().Trim();
                    if (!string.IsNullOrWhiteSpace(familia))
                    {
                        try
                        {
                            if (familiaValuesDict.TryGetValue(familia.ToLower(), out var familiaId))
                            {
                                await _productRepository.CreateProductPropertyAsync(productResponse.IdProduct, familiaId);
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

                    // Columna F: Clase - buscar en diccionario precargado
                    var clase = worksheet.Cell(row, 6).GetValue<string>().Trim();
                    if (!string.IsNullOrWhiteSpace(clase))
                    {
                        try
                        {
                            if (claseValuesDict.TryGetValue(clase.ToLower(), out var claseId))
                            {
                                await _productRepository.CreateProductPropertyAsync(productResponse.IdProduct, claseId);
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

                    // Columna G: Línea - buscar en diccionario precargado
                    var linea = worksheet.Cell(row, 7).GetValue<string>().Trim();
                    if (!string.IsNullOrWhiteSpace(linea))
                    {
                        try
                        {
                            if (lineaValuesDict.TryGetValue(linea.ToLower(), out var lineaId))
                            {
                                await _productRepository.CreateProductPropertyAsync(productResponse.IdProduct, lineaId);
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