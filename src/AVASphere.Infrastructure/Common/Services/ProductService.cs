using AVASphere.ApplicationCore.Common.DTOs.ProductDTOs;
using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Projects.Entities.jsons;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using ExcelDataReader;
using System.Data;
using System.Text;

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

    /// <summary>
    /// Agrega una URL de imagen al array de imágenes del producto
    /// </summary>
    public async Task<bool> AddProductImageAsync(int idProduct, string imageUrl)
    {
        var product = await _productRepository.GetByIdProductsAsync(idProduct);
        if (product == null)
        {
            throw new KeyNotFoundException($"Producto con ID {idProduct} no encontrado.");
        }

        if (product.ImageUrls == null)
        {
            product.ImageUrls = new List<ProductImageJson>();
        }

        // Obtener el índice más alto actual
        var maxIndex = product.ImageUrls.Any() ? product.ImageUrls.Max(i => i.Index) : -1;

        // Evitar duplicados por URL
        if (!product.ImageUrls.Any(i => i.Url == imageUrl))
        {
            var imageExtension = Path.GetExtension(imageUrl);
            var contentType = imageExtension.ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };

            product.ImageUrls.Add(new ProductImageJson
            {
                Index = maxIndex + 1,
                Url = imageUrl,
                FileName = Path.GetFileName(imageUrl),
                ContentType = contentType,
                IsMain = !product.ImageUrls.Any() // Primera imagen es la principal
            });
            await _productRepository.UpdateProductsAsync(product);
        }

        return true;
    }

    /// <summary>
    /// Elimina una URL de imagen del array de imágenes del producto
    /// </summary>
    public async Task<bool> RemoveProductImageAsync(int idProduct, string imageUrl)
    {
        var product = await _productRepository.GetByIdProductsAsync(idProduct);
        if (product == null)
        {
            throw new KeyNotFoundException($"Producto con ID {idProduct} no encontrado.");
        }

        if (product.ImageUrls != null)
        {
            var imageToRemove = product.ImageUrls.FirstOrDefault(i => i.Url == imageUrl);
            if (imageToRemove != null)
            {
                product.ImageUrls.Remove(imageToRemove);
                await _productRepository.UpdateProductsAsync(product);
            }
        }

        return true;
    }

    public async Task<ProductResponseDto?> GetProductByIdAsync(int id, ProductFilterDto? filters = null)
    {
        var product = await _productRepository.GetByIdProductsAsync(id, filters);

        if (product == null)
            return null;

        return MapToResponseDto(product);
    }

    public async Task<bool?> IsProductHerrajeByCodeAsync(string principalCode)
    {
        return await IsProductHerrajeByCodeOrNameAsync(principalCode, null);
    }

    public async Task<bool?> IsProductHerrajeByCodeOrNameAsync(string? principalCode, string? mainName)
    {
        if (string.IsNullOrWhiteSpace(principalCode) && string.IsNullOrWhiteSpace(mainName))
        {
            throw new ArgumentException("Debe proporcionar el codigo o el nombre del producto.");
        }

        Product? product = null;
        if (!string.IsNullOrWhiteSpace(principalCode))
        {
            product = await _productRepository.GetByPrincipalCodeAsync(principalCode);
        }

        if (product == null && !string.IsNullOrWhiteSpace(mainName))
        {
            product = await _productRepository.GetByMainNameAsync(mainName);
        }

        if (product == null)
        {
            return null;
        }

        return product.ProductProperties.Any(pp =>
            pp.PropertyValue != null &&
            (string.Equals(pp.PropertyValue.Type, "Herrajes", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(pp.PropertyValue.Type, "Area de Herrajes", StringComparison.OrdinalIgnoreCase)) &&
            string.Equals(pp.PropertyValue.Property?.Name, "Familia", StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IReadOnlyList<HerrajeLookupResultDto>> GetHerrajeStatusByCodesAsync(IEnumerable<string> principalCodes)
    {
        if (principalCodes == null)
        {
            throw new ArgumentNullException(nameof(principalCodes));
        }

        var normalizedCodes = principalCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .ToList();

        if (normalizedCodes.Count == 0)
        {
            return Array.Empty<HerrajeLookupResultDto>();
        }

        var distinctCodes = normalizedCodes
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var codeSet = new HashSet<string>(distinctCodes, StringComparer.OrdinalIgnoreCase);
        var products = await _productRepository.GetByPrincipalCodesAsync(distinctCodes);

        var codeToProduct = new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase);
        foreach (var product in products)
        {
            foreach (var codeJson in product.CodeJson.Where(c =>
                         string.Equals(c.Type, "Principal", StringComparison.OrdinalIgnoreCase) &&
                         !string.IsNullOrWhiteSpace(c.Code)))
            {
                var trimmedCode = codeJson.Code!.Trim();
                if (codeSet.Contains(trimmedCode) && !codeToProduct.ContainsKey(trimmedCode))
                {
                    codeToProduct[trimmedCode] = product;
                }
            }
        }

        return distinctCodes
            .Select(code =>
            {
                if (!codeToProduct.TryGetValue(code, out var product))
                {
                    return new HerrajeLookupResultDto
                    {
                        Code = code,
                        Found = false,
                        IsHerraje = false
                    };
                }

                return new HerrajeLookupResultDto
                {
                    Code = code,
                    Found = true,
                    IsHerraje = IsHerrajeProduct(product),
                    MainName = product.MainName
                };
            })
            .ToList();
    }

    private static bool IsHerrajeProduct(Product product)
    {
        return product.ProductProperties.Any(pp =>
            pp.PropertyValue != null &&
            (string.Equals(pp.PropertyValue.Type, "Herrajes", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(pp.PropertyValue.Type, "Area de Herrajes", StringComparison.OrdinalIgnoreCase)) &&
            string.Equals(pp.PropertyValue.Property?.Name, "Familia", StringComparison.OrdinalIgnoreCase));
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
            ImageUrls = product.ImageUrls?.ToList() ?? new List<ProductImageJson>(),
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
    /// Importa productos desde un archivo Excel en uno de dos formatos.
    /// 
    /// Formato 1 (original):
    /// - Columna A: Código (requerido)
    /// - Columna B: Descripción (requerido)
    /// - Columna C: Unidad (opcional, por defecto "S/U" si está vacío)
    /// - Columna D: Activo (requerido, True/False o 1/0, solo importa si es True)
    /// - Columna E-H: Reservadas (no utilizadas, pueden estar vacías)
    /// - Columna I: Proveedor (requerido, se usa ID 37 por defecto si no existe)
    /// - Columna J: Familia (opcional, debe existir en PropertyValues)
    /// - Columna K: Clase (opcional, debe existir en PropertyValues)
    /// - Columna L: Línea (opcional, debe existir en PropertyValues)
    /// 
    /// Formato 2 (catalogo):
    /// - Columna C: Codigo producto
    /// - Columna D: Codigo SAT
    /// - Columna E: Codigo proveedor
    /// - Columna F: Descripcion
    /// - Columna H: Unidad
    /// - Columna W/X: Id y nombre de Familia
    /// - Columna Y/Z: Id y nombre de Clase
    /// - Columna AA/AB: Id y nombre de Linea
    /// - Columna AT/AU: Id y nombre de Proveedor
    /// 
    /// Las filas de encabezado se saltan automáticamente en formato 1. 
    /// Se procesa en lotes de 400 productos para optimizar el rendimiento.
    /// </summary>
    public async Task<ImportProductResultDto> ImportProductsFromExcelAsync(Stream excelStream)
    {
        using var workbook = await LoadWorkbookFromExcelAsync(excelStream);
        var worksheet = workbook.Worksheet(1);
        var format = DetectImportExcelFormat(worksheet);

        return format == ImportExcelFormat.CatalogV2
            ? await ImportProductsFromCatalogFormatAsync(worksheet)
            : await ImportProductsFromOriginalFormatAsync(worksheet);
    }

    private static async Task<XLWorkbook> LoadWorkbookFromExcelAsync(Stream excelStream)
    {
        if (excelStream == null)
        {
            throw new ArgumentNullException(nameof(excelStream));
        }

        using var buffer = new MemoryStream();
        await excelStream.CopyToAsync(buffer);
        var data = buffer.ToArray();

        if (IsOleCompoundDocument(data))
        {
            return LoadWorkbookFromXls(data);
        }

        return new XLWorkbook(new MemoryStream(data));
    }

    private static XLWorkbook LoadWorkbookFromXls(byte[] data)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using var stream = new MemoryStream(data);
        using var reader = ExcelReaderFactory.CreateReader(stream);
        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = false
            }
        });

        if (dataSet.Tables.Count == 0)
        {
            throw new InvalidDataException("El archivo Excel no contiene hojas legibles.");
        }

        var workbook = new XLWorkbook();
        workbook.Worksheets.Add(dataSet.Tables[0], "Sheet1");
        return workbook;
    }

    private static bool IsOleCompoundDocument(byte[] data)
    {
        if (data.Length < 8)
        {
            return false;
        }

        return data[0] == 0xD0 && data[1] == 0xCF && data[2] == 0x11 && data[3] == 0xE0 &&
               data[4] == 0xA1 && data[5] == 0xB1 && data[6] == 0x1A && data[7] == 0xE1;
    }

    private enum ImportExcelFormat
    {
        Original,
        CatalogV2
    }

    private static ImportExcelFormat DetectImportExcelFormat(IXLWorksheet worksheet)
    {
        if (FindCatalogHeaderRow(worksheet).HasValue)
        {
            return ImportExcelFormat.CatalogV2;
        }

        return ImportExcelFormat.Original;
    }

    private static int? FindCatalogHeaderRow(IXLWorksheet worksheet)
    {
        for (int row = 1; row <= 5; row++)
        {
            var headerA = worksheet.Cell(row, 1).GetValue<string>().Trim();
            var headerC = worksheet.Cell(row, 3).GetValue<string>().Trim();
            var headerF = worksheet.Cell(row, 6).GetValue<string>().Trim();

            if (headerA.Equals("nuevocodigo", StringComparison.OrdinalIgnoreCase) &&
                headerC.Equals("id", StringComparison.OrdinalIgnoreCase) &&
                headerF.Equals("descripcion", StringComparison.OrdinalIgnoreCase))
            {
                return row;
            }
        }

        return null;
    }

    private async Task<int?> EnsurePropertyValueAsync(
        string propertyName,
        string propertyValue,
        Dictionary<string, int> cache,
        ImportProductResultDto result,
        int row)
    {
        if (string.IsNullOrWhiteSpace(propertyValue))
        {
            return null;
        }

        var normalizedValue = propertyValue.Trim().ToLowerInvariant();
        if (cache.TryGetValue(normalizedValue, out var cachedId))
        {
            return cachedId;
        }

        var createdId = await _productRepository.GetOrCreatePropertyValueIdAsync(propertyName, propertyValue.Trim());
        cache[normalizedValue] = createdId;
        result.Errors.Add($"Fila {row}: PropertyValue '{propertyName}' con valor '{propertyValue}' no encontrado. Se creó automáticamente.");
        return createdId;
    }

    private async Task<ImportProductResultDto> ImportProductsFromOriginalFormatAsync(IXLWorksheet worksheet)
    {
        var result = new ImportProductResultDto();
        const int batchSize = 400;

        // PRE-CARGAR TODOS LOS DATOS NECESARIOS ANTES DEL BUCLE
        var suppliersDict = await _productRepository.GetAllSuppliersAsync();
        var familiaValuesDict = await _productRepository.GetPropertyValueIdsByPropertyNameAsync("Familia");
        var claseValuesDict = await _productRepository.GetPropertyValueIdsByPropertyNameAsync("Clase");
        var lineaValuesDict = await _productRepository.GetPropertyValueIdsByPropertyNameAsync("Línea");
        var batch = new List<(int Row, Product Product)>(batchSize);

        var lastRow = worksheet.LastRowUsed();
        if (lastRow == null)
        {
            return result;
        }

        var rowCount = lastRow.RowNumber();

        for (int row = 2; row <= rowCount; row++)
        {
            try
            {
                // Columna A: Código
                var code = worksheet.Cell(row, 1).GetValue<string>().Trim();

                // Columna B: Descripción
                var description = worksheet.Cell(row, 2).GetValue<string>().Trim();

                if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(description))
                {
                    continue;
                }

                // ✅ VALIDACIÓN: Saltar filas de encabezado (títulos del Excel)
                var commonHeaders = new[] { "codigo", "código", "code", "descripcion", "descripción", "description", "unidad", "unit", "activo", "active", "proveedor", "supplier" };
                var isHeaderRow = (!string.IsNullOrWhiteSpace(code) && commonHeaders.Contains(code.ToLower())) ||
                                 (!string.IsNullOrWhiteSpace(description) && commonHeaders.Contains(description.ToLower()));

                if (isHeaderRow)
                {
                    continue;
                }

                // Columna C: Unidad (por defecto "S/N" si está vacío)
                var unit = worksheet.Cell(row, 3).GetValue<string>().Trim();
                if (string.IsNullOrWhiteSpace(unit))
                {
                    unit = "S/U";
                }

                // Columna D: Activo (True/False) - VERIFICACIÓN OBLIGATORIA
                var activoCell = worksheet.Cell(row, 4);
                bool activo = false;

                // Intentar leer como booleano o como string
                try
                {
                    activo = activoCell.GetValue<bool>();
                }
                catch
                {
                    var activoString = activoCell.GetValue<string>().Trim();
                    activo = activoString.Equals("True", StringComparison.OrdinalIgnoreCase) ||
                             activoString.Equals("1", StringComparison.OrdinalIgnoreCase);
                }

                // Si Activo es False, ignorar esta fila y continuar con la siguiente
                if (!activo)
                {
                    result.Errors.Add($"Fila {row}: Producto '{code}' omitido (Activo = False)");
                    continue;
                }

                // Columna I (9): Proveedor
                var supplierName = worksheet.Cell(row, 9).GetValue<string>().Trim();
                int supplierId;

                // Buscar proveedor en el diccionario precargado, si no existe usar ID 37 por defecto
                if (!suppliersDict.TryGetValue(supplierName.ToLower(), out var supplier))
                {
                    result.Errors.Add($"Fila {row}: Proveedor '{supplierName}' no encontrado. Se usará Supplier ID 37 por defecto.");
                    supplierId = 37;
                }
                else
                {
                    supplierId = supplier.IdSupplier;
                }

                var product = new Product
                {
                    MainName = description,
                    Unit = unit,
                    Description = description,
                    Quantity = 0,
                    Taxes = 16,
                    IdSupplier = supplierId,
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
                    SolutionsJsons = new List<SolutionsJson>(),
                    ProductProperties = new List<ProductProperties>()
                };

                // Columna J (10): Familia - buscar en diccionario precargado
                var familia = worksheet.Cell(row, 10).GetValue<string>().Trim();
                var familiaId = await EnsurePropertyValueAsync("Familia", familia, familiaValuesDict, result, row);
                if (familiaId.HasValue)
                {
                    product.ProductProperties.Add(new ProductProperties
                    {
                        IdPropertyValue = familiaId.Value
                    });
                }

                // Columna K (11): Clase - buscar en diccionario precargado
                var clase = worksheet.Cell(row, 11).GetValue<string>().Trim();
                var claseId = await EnsurePropertyValueAsync("Clase", clase, claseValuesDict, result, row);
                if (claseId.HasValue)
                {
                    product.ProductProperties.Add(new ProductProperties
                    {
                        IdPropertyValue = claseId.Value
                    });
                }

                // Columna L (12): Línea - buscar en diccionario precargado
                var linea = worksheet.Cell(row, 12).GetValue<string>().Trim();
                var lineaId = await EnsurePropertyValueAsync("Línea", linea, lineaValuesDict, result, row);
                if (lineaId.HasValue)
                {
                    product.ProductProperties.Add(new ProductProperties
                    {
                        IdPropertyValue = lineaId.Value
                    });
                }

                batch.Add((row, product));
                if (batch.Count >= batchSize)
                {
                    await PersistImportBatchAsync(batch, result);
                    batch.Clear();
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Fila {row}: {ex.Message}");
                result.FailedImports++;
            }
        }

        if (batch.Count > 0)
        {
            await PersistImportBatchAsync(batch, result);
        }

        result.TotalRows = rowCount - 1;
        return result;
    }

    private async Task<ImportProductResultDto> ImportProductsFromCatalogFormatAsync(IXLWorksheet worksheet)
    {
        var result = new ImportProductResultDto();
        const int batchSize = 400;

        var suppliersDict = await _productRepository.GetAllSuppliersAsync();
        var familiaValuesDict = await _productRepository.GetPropertyValueIdsByPropertyNameAsync("Familia");
        var claseValuesDict = await _productRepository.GetPropertyValueIdsByPropertyNameAsync("Clase");
        var lineaValuesDict = await _productRepository.GetPropertyValueIdsByPropertyNameAsync("Línea");
        var batch = new List<(int Row, Product Product)>(batchSize);

        var lastRow = worksheet.LastRowUsed();
        if (lastRow == null)
        {
            return result;
        }

        var rowCount = lastRow.RowNumber();

        var headerRow = FindCatalogHeaderRow(worksheet) ?? 1;
        var startRow = headerRow + 1;

        for (int row = startRow; row <= rowCount; row++)
        {
            try
            {
                var productCode = worksheet.Cell(row, 3).GetValue<string>().Trim();
                var satCode = worksheet.Cell(row, 4).GetValue<string>().Trim();
                var supplierCode = worksheet.Cell(row, 5).GetValue<string>().Trim();
                var description = worksheet.Cell(row, 6).GetValue<string>().Trim();
                var unit = worksheet.Cell(row, 8).GetValue<string>().Trim();

                if (string.IsNullOrWhiteSpace(productCode) && string.IsNullOrWhiteSpace(description))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(unit))
                {
                    unit = "S/U";
                }

                var supplierName = worksheet.Cell(row, 47).GetValue<string>().Trim();
                int supplierId;

                if (!string.IsNullOrWhiteSpace(supplierName) && suppliersDict.TryGetValue(supplierName.ToLower(), out var supplier))
                {
                    supplierId = supplier.IdSupplier;
                }
                else
                {
                    result.Errors.Add($"Fila {row}: Proveedor '{supplierName}'/{supplierCode} no encontrado. Se usará Supplier ID 37 por defecto.");
                    supplierId = 37;
                }

                var taxes = 16;
                var taxCell = worksheet.Cell(row, 31).GetValue<string>().Trim();
                if (double.TryParse(taxCell, out var taxValue))
                {
                    taxes = Convert.ToInt32(Math.Round(taxValue));
                }

                var codes = new List<CodeJson>();
                if (!string.IsNullOrWhiteSpace(productCode))
                {
                    codes.Add(new CodeJson
                    {
                        Index = codes.Count,
                        Type = "Principal",
                        Code = productCode
                    });
                }

                if (!string.IsNullOrWhiteSpace(satCode))
                {
                    codes.Add(new CodeJson
                    {
                        Index = codes.Count,
                        Type = "SAT",
                        Code = satCode
                    });
                }

                if (!string.IsNullOrWhiteSpace(supplierCode))
                {
                    codes.Add(new CodeJson
                    {
                        Index = codes.Count,
                        Type = "Proveedor",
                        Code = supplierCode
                    });
                }

                var product = new Product
                {
                    MainName = description,
                    Unit = unit,
                    Description = description,
                    Quantity = 0,
                    Taxes = taxes,
                    IdSupplier = supplierId,
                    CodeJson = codes,
                    CostsJson = new List<CostsJson>(),
                    CategoriesJsons = new List<CategoriesJson>(),
                    SolutionsJsons = new List<SolutionsJson>(),
                    ProductProperties = new List<ProductProperties>()
                };

                var familiaName = worksheet.Cell(row, 24).GetValue<string>().Trim();
                var familiaId = await EnsurePropertyValueAsync("Familia", familiaName, familiaValuesDict, result, row);
                if (familiaId.HasValue)
                {
                    product.ProductProperties.Add(new ProductProperties
                    {
                        IdPropertyValue = familiaId.Value
                    });
                }

                var claseName = worksheet.Cell(row, 26).GetValue<string>().Trim();
                var claseId = await EnsurePropertyValueAsync("Clase", claseName, claseValuesDict, result, row);
                if (claseId.HasValue)
                {
                    product.ProductProperties.Add(new ProductProperties
                    {
                        IdPropertyValue = claseId.Value
                    });
                }

                var lineaName = worksheet.Cell(row, 28).GetValue<string>().Trim();
                var lineaId = await EnsurePropertyValueAsync("Línea", lineaName, lineaValuesDict, result, row);
                if (lineaId.HasValue)
                {
                    product.ProductProperties.Add(new ProductProperties
                    {
                        IdPropertyValue = lineaId.Value
                    });
                }

                batch.Add((row, product));
                if (batch.Count >= batchSize)
                {
                    await PersistImportBatchAsync(batch, result);
                    batch.Clear();
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Fila {row}: {ex.Message}");
                result.FailedImports++;
            }
        }

        if (batch.Count > 0)
        {
            await PersistImportBatchAsync(batch, result);
        }

        result.TotalRows = rowCount - 1;
        return result;
    }

    private async Task PersistImportBatchAsync(List<(int Row, Product Product)> batch, ImportProductResultDto result)
    {
        if (batch.Count == 0)
        {
            return;
        }

        try
        {
            await _productRepository.CreateProductsBulkAsync(batch.Select(x => x.Product).ToList());
            result.SuccessfulImports += batch.Count;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Lote con {batch.Count} filas falló: {ex.Message}. Se reintentará fila por fila.");

            foreach (var item in batch)
            {
                try
                {
                    await _productRepository.CreateProductsAsync(item.Product);
                    result.SuccessfulImports++;
                }
                catch (Exception rowEx)
                {
                    result.Errors.Add($"Fila {item.Row}: {rowEx.Message}");
                    result.FailedImports++;
                }
            }
        }
    }

}
