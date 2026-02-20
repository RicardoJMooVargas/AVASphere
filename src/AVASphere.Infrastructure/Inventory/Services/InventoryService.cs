using AVASphere.ApplicationCore.Inventory.DTOs;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Common.DTOs.ProductDTOs;
using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Projects.Entities.jsons;
using InventoryEntity = AVASphere.ApplicationCore.Inventory.Entities.General.Inventory;
using PhysicalInventoryEntity = AVASphere.ApplicationCore.Inventory.Entities.General.PhysicalInventory;
using LocationDetailsEntity = AVASphere.ApplicationCore.Inventory.Entities.General.LocationDetails;
using StorageStructureEntity = AVASphere.ApplicationCore.Inventory.Entities.General.StorageStructure;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Inventory.Services;

/// <summary>
/// Servicio para gestión de inventario
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPhysicalInventoryRepository _physicalInventoryRepository;
    private readonly ILocationDetailsRepository _locationDetailsRepository;
    private readonly IStorageStructureRepository _storageStructureRepository;
    private readonly IProductService _productService;
    private readonly MasterDbContext _context;

    public InventoryService(
        IInventoryRepository inventoryRepository,
        IWarehouseRepository warehouseRepository,
        IProductRepository productRepository,
        IPhysicalInventoryRepository physicalInventoryRepository,
        ILocationDetailsRepository locationDetailsRepository,
        IStorageStructureRepository storageStructureRepository,
        IProductService productService,
        MasterDbContext context)
    {
        _inventoryRepository = inventoryRepository;
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
        _physicalInventoryRepository = physicalInventoryRepository;
        _locationDetailsRepository = locationDetailsRepository;
        _storageStructureRepository = storageStructureRepository;
        _productService = productService;
        _context = context;
    }

    /// <summary>
    /// Importa inventario desde un archivo Excel
    /// </summary>
    public async Task<ImportInventoryResultDto> ImportInventoryFromExcelAsync(Stream excelStream)
    {
        var result = new ImportInventoryResultDto();

        // Diccionario para cachear bodegas
        var warehousesCache = new Dictionary<string, int>();

        // Pre-cargar todas las bodegas activas
        var warehouses = await _warehouseRepository.GetActiveWarehousesAsync();
        foreach (var warehouse in warehouses)
        {
            warehousesCache[warehouse.Code] = warehouse.IdWarehouse;
        }

        // Pre-cargar todos los productos con su descripción en memoria
        var allProducts = await _context.Products.ToListAsync();
        var productsByDescription = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var product in allProducts)
        {
            // Usar MainName como clave (case-insensitive para mejor matching)
            if (!string.IsNullOrWhiteSpace(product.MainName))
            {
                productsByDescription[product.MainName.Trim()] = product.IdProduct;
            }
        }

        // Obtener o crear PhysicalInventory por cada bodega
        var physicalInventoryCache = new Dictionary<int, int>(); // IdWarehouse -> IdPhysicalInventory

        foreach (var warehouse in warehouses)
        {
            // Buscar un PhysicalInventory activo para esta bodega
            var physicalInventories = await _physicalInventoryRepository.GetByWarehouseIdAsync(warehouse.IdWarehouse);
            var activePhysicalInventory = physicalInventories
                .FirstOrDefault(pi => pi.Status == "Open");

            if (activePhysicalInventory == null)
            {
                // Crear un PhysicalInventory por defecto para esta bodega
                var newPhysicalInventory = new PhysicalInventoryEntity
                {
                    IdWarehouse = warehouse.IdWarehouse,
                    InventoryDate = DateTime.UtcNow,
                    Status = "Open",
                    CreatedBy = 1, // Usuario sistema por defecto
                    Observations = "Inventario generado automáticamente por importación Excel"
                };

                activePhysicalInventory = await _physicalInventoryRepository.CreateAsync(newPhysicalInventory);
            }

            physicalInventoryCache[warehouse.IdWarehouse] = activePhysicalInventory.IdPhysicalInventory;
        }

        using (var workbook = new XLWorkbook(excelStream))
        {
            var worksheet = workbook.Worksheet(1);
            var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            if (rowCount < 2)
            {
                result.Errors.Add("El archivo Excel está vacío o no tiene datos");
                return result;
            }

            // Estructura para agrupar inventarios por Producto + Ubicación (sumando todas las bodegas)
            var inventoryGroups = new Dictionary<string, InventoryGroup>();

            // Primera pasada: Leer todas las filas y agrupar
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    // Columna B: Descripción del producto (para matching)
                    var descripcion = worksheet.Cell(row, 2).GetValue<string>()?.Trim();

                    if (string.IsNullOrWhiteSpace(descripcion))
                    {
                        result.Warnings.Add($"Fila {row}: Descripción vacía, se omite");
                        result.TotalRows++;
                        continue;
                    }

                    // Buscar el producto por descripción
                    if (!productsByDescription.TryGetValue(descripcion, out int idProduct))
                    {
                        result.ProductsNotFound++;
                        result.Errors.Add($"Fila {row}: Producto '{descripcion}' no encontrado");
                        result.FailedImports++;
                        result.TotalRows++;
                        continue;
                    }

                    // Columna M (13): Ubicación
                    var ubicacion = ReadIntValue(worksheet.Cell(row, 13));

                    // Leer las cantidades de cada bodega (Columnas E-H: 5-8)
                    var stocks = new[]
                    {
                        new { Code = "AVA01", Stock = ReadDoubleValue(worksheet.Cell(row, 5)), IdWarehouse = warehousesCache.GetValueOrDefault("AVA01", 0) },
                        new { Code = "AVA02", Stock = ReadDoubleValue(worksheet.Cell(row, 6)), IdWarehouse = warehousesCache.GetValueOrDefault("AVA02", 0) },
                        new { Code = "AVA03", Stock = ReadDoubleValue(worksheet.Cell(row, 7)), IdWarehouse = warehousesCache.GetValueOrDefault("AVA03", 0) },
                        new { Code = "AVA04", Stock = ReadDoubleValue(worksheet.Cell(row, 8)), IdWarehouse = warehousesCache.GetValueOrDefault("AVA04", 0) }
                    };

                    // Sumar todas las cantidades de todas las bodegas
                    var totalStock = stocks.Sum(s => s.Stock);

                    // Determinar la bodega según el número de ubicación (Ubicación 1→AVA01, 2→AVA02, etc.)
                    var warehouseCode = $"AVA0{ubicacion}";
                    var idWarehouse = warehousesCache.GetValueOrDefault(warehouseCode, 0);

                    // Si la ubicación no corresponde a ninguna bodega, usar la primera con stock
                    if (idWarehouse == 0)
                    {
                        var fallbackWarehouse = stocks.FirstOrDefault(s => s.Stock > 0 && s.IdWarehouse > 0)
                                             ?? stocks.FirstOrDefault(s => s.IdWarehouse > 0);

                        if (fallbackWarehouse == null || fallbackWarehouse.IdWarehouse == 0)
                        {
                            result.WarehousesNotFound++;
                            result.Warnings.Add($"Fila {row}: No se encontró ninguna bodega válida");
                            result.TotalRows++;
                            continue;
                        }

                        idWarehouse = fallbackWarehouse.IdWarehouse;
                        warehouseCode = fallbackWarehouse.Code;
                    }

                    // Crear clave única: IdProduct + Ubicación (sin bodega)
                    var groupKey = $"{idProduct}_{ubicacion}";

                    if (inventoryGroups.ContainsKey(groupKey))
                    {
                        // Ya existe este grupo, sumar el stock
                        inventoryGroups[groupKey].TotalStock = (inventoryGroups[groupKey].TotalStock ?? 0) + totalStock;
                    }
                    else
                    {
                        // Crear nuevo grupo
                        inventoryGroups[groupKey] = new InventoryGroup
                        {
                            IdProduct = idProduct,
                            IdWarehouse = idWarehouse, // Usar la bodega correspondiente a la ubicación
                            LocationDetail = ubicacion,
                            TotalStock = totalStock, // Suma de todas las bodegas
                            ProductDescription = descripcion,
                            WarehouseCode = warehouseCode
                        };
                    }

                    result.TotalRows++;
                }
                catch (Exception ex)
                {
                    var errorMessage = ex.InnerException != null
                        ? $"{ex.Message} - Inner: {ex.InnerException.Message}"
                        : ex.Message;

                    result.Errors.Add($"Fila {row}: Error al leer - {errorMessage}");
                    result.FailedImports++;
                    result.TotalRows++;
                }
            }

            // Segunda pasada: Crear/Actualizar inventarios agrupados
            foreach (var group in inventoryGroups.Values)
            {
                try
                {
                    // Verificar si ya existe inventario para este producto en esta bodega
                    var existingInventory = await _inventoryRepository.GetByWarehouseAndProductAsync(
                        group.IdWarehouse,
                        group.IdProduct);

                    if (existingInventory != null)
                    {
                        // Actualizar el stock y ubicación existente
                        existingInventory.Stock = group.TotalStock.GetValueOrDefault();
                        existingInventory.LocationDetail = group.LocationDetail.GetValueOrDefault();
                        await _inventoryRepository.UpdateAsync(existingInventory);

                        result.Warnings.Add($"Actualizado: {group.ProductDescription} en {group.WarehouseCode}, Ubicación {group.LocationDetail}, Stock: {group.TotalStock}");
                    }
                    else
                    {
                        // Crear nuevo registro de inventario
                        var inventory = new InventoryEntity
                        {
                            IdProduct = group.IdProduct,
                            IdWarehouse = group.IdWarehouse,
                            Stock = group.TotalStock.GetValueOrDefault(),
                            StockMin = 0,
                            StockMax = group.TotalStock.GetValueOrDefault() * 2,
                            LocationDetail = group.LocationDetail.GetValueOrDefault(),
                            IdPhysicalInventory = physicalInventoryCache[group.IdWarehouse]
                        };

                        await _inventoryRepository.CreateAsync(inventory);

                        result.SuccessfulImports++;
                        result.CreatedRecords.Add($"{group.ProductDescription} | {group.WarehouseCode} | Ubicación: {group.LocationDetail} | Stock: {group.TotalStock}");
                    }
                }
                catch (Exception ex)
                {
                    var errorMessage = ex.InnerException != null
                        ? $"{ex.Message} - Inner: {ex.InnerException.Message}"
                        : ex.Message;

                    result.Errors.Add($"Error al guardar {group.ProductDescription} en {group.WarehouseCode}: {errorMessage}");
                    result.FailedImports++;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Importa inventario de ubicación desde un archivo Excel
    /// </summary>
    public async Task<ImportInventoryResultDto> ImportInventoryUbicationFromExcelAsync(Stream excelStream)
    {
        var result = new ImportInventoryResultDto();

        // Pre-cargar todos los productos con su código y descripción en memoria
        var allProducts = await _context.Products.ToListAsync();
        var productsByCode = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var productsByDescription = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var product in allProducts)
        {
            // Intentar obtener el código desde CodeJson
            if (product.CodeJson != null && product.CodeJson.Any())
            {
                var firstCode = product.CodeJson.FirstOrDefault()?.Code;
                if (!string.IsNullOrWhiteSpace(firstCode))
                {
                    var trimmedCode = firstCode.Trim();

                    // Agregar el código completo
                    productsByCode[trimmedCode] = product.IdProduct;

                    // Si el código contiene guiones, agregar también la parte después del último guion
                    // Para manejar casos como "320-2220000BL" donde el Excel solo trae "2220000BL"
                    if (trimmedCode.Contains("-"))
                    {
                        var codeParts = trimmedCode.Split('-');
                        var lastPart = codeParts[codeParts.Length - 1].Trim();
                        if (!string.IsNullOrWhiteSpace(lastPart) && !productsByCode.ContainsKey(lastPart))
                        {
                            productsByCode[lastPart] = product.IdProduct;
                        }
                    }
                }
            }

            // Usar MainName como clave (case-insensitive para mejor matching)
            if (!string.IsNullOrWhiteSpace(product.MainName))
            {
                productsByDescription[product.MainName.Trim()] = product.IdProduct;
            }
        }

        using (var workbook = new XLWorkbook(excelStream))
        {
            var worksheet = workbook.Worksheet(1);
            var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;

            if (rowCount < 2)
            {
                result.Errors.Add("El archivo Excel está vacío o no tiene datos");
                return result;
            }

            // Procesar cada fila
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    // Columna A: Código
                    var codigo = worksheet.Cell(row, 1).GetValue<string>()?.Trim();

                    // Columna B: Descripción
                    var descripcion = worksheet.Cell(row, 2).GetValue<string>()?.Trim();

                    // Columna M (13): Ubicación (ej: "HERRAJES HERRALUM 1 RACK 1")
                    var ubicacion = worksheet.Cell(row, 13).GetValue<string>()?.Trim();

                    // Columna N (14): Nivel (ej: "A")
                    var nivelStr = worksheet.Cell(row, 14).GetValue<string>()?.Trim();

                    // Validar que tenga al menos código o descripción
                    if (string.IsNullOrWhiteSpace(codigo) && string.IsNullOrWhiteSpace(descripcion))
                    {
                        result.Warnings.Add($"Fila {row}: No tiene código ni descripción, se omite");
                        result.TotalRows++;
                        continue;
                    }

                    // Buscar el producto por código O descripción
                    int? idProduct = null;

                    // Intentar buscar por código primero
                    if (!string.IsNullOrWhiteSpace(codigo) && productsByCode.TryGetValue(codigo, out int idByCode))
                    {
                        idProduct = idByCode;
                    }
                    // Si no encontró por código, intentar por descripción
                    else if (!string.IsNullOrWhiteSpace(descripcion) && productsByDescription.TryGetValue(descripcion, out int idByDesc))
                    {
                        idProduct = idByDesc;
                    }

                    // Si no se encontró el producto, crearlo automáticamente
                    if (!idProduct.HasValue)
                    {
                        try
                        {
                            var createProductDto = new CreateProductDto
                            {
                                MainName = descripcion ?? "Producto generado automáticamente",
                                Unit = "Otro",
                                Description = descripcion ?? "Producto generado automáticamente",
                                Quantity = 0,
                                Taxes = 16,
                                IdSupplier = 37,
                                CodeJson = new List<CodeJson>
                                {
                                    new CodeJson
                                    {
                                        Index = 0,
                                        Type = "Principal",
                                        Code = codigo ?? "Sin código"
                                    }
                                },
                                CostsJson = new List<CostsJson>(),
                                CategoriesJsons = new List<CategoriesJson>(),
                                SolutionsJsons = new List<SolutionsJson>()
                            };

                            var createdProduct = await _productService.CreateProductAsync(createProductDto);
                            idProduct = createdProduct.IdProduct;

                            // Agregar el nuevo producto a los diccionarios para búsquedas futuras
                            if (!string.IsNullOrWhiteSpace(codigo))
                            {
                                productsByCode[codigo] = idProduct.Value;

                                // Si contiene guiones, agregar también la parte final
                                if (codigo.Contains("-"))
                                {
                                    var codeParts = codigo.Split('-');
                                    var lastPart = codeParts[codeParts.Length - 1].Trim();
                                    if (!string.IsNullOrWhiteSpace(lastPart) && !productsByCode.ContainsKey(lastPart))
                                    {
                                        productsByCode[lastPart] = idProduct.Value;
                                    }
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(descripcion))
                            {
                                productsByDescription[descripcion] = idProduct.Value;
                            }

                            result.Warnings.Add($"Fila {row}: Producto no encontrado, se creó automáticamente (ID: {idProduct.Value})");
                        }
                        catch (Exception ex)
                        {
                            result.ProductsNotFound++;
                            result.Errors.Add($"Fila {row}: No se pudo crear el producto automáticamente - {ex.Message}");
                            result.FailedImports++;
                            result.TotalRows++;
                            continue;
                        }
                    }

                    // Validar que tenga ubicación
                    if (string.IsNullOrWhiteSpace(ubicacion))
                    {
                        result.Warnings.Add($"Fila {row}: Producto encontrado pero sin ubicación, se omite");
                        result.TotalRows++;
                        continue;
                    }

                    // Si no tiene nivel, usar "S/N" y buscar el StorageStructure correspondiente
                    StorageStructureEntity storageStructure;

                    if (string.IsNullOrWhiteSpace(nivelStr))
                    {
                        nivelStr = "S/N";
                        // Buscar StorageStructure con CodeRack "S/N"
                        storageStructure = await _storageStructureRepository.GetByCodeAsync("S/N");

                        if (storageStructure == null)
                        {
                            result.Warnings.Add($"Fila {row}: No se encontró StorageStructure con CodeRack 'S/N', se omite");
                            result.FailedImports++;
                            result.TotalRows++;
                            continue;
                        }

                        result.Warnings.Add($"Fila {row}: Sin nivel, se asignó 'S/N' automáticamente");
                    }
                    else
                    {
                        // Buscar StorageStructure por CodeRack que coincida con Ubicación
                        storageStructure = await _storageStructureRepository.GetByCodeAsync(ubicacion);
                    }

                    if (storageStructure == null)
                    {
                        result.Warnings.Add($"Fila {row}: No se encontró StorageStructure con CodeRack '{ubicacion}'");
                        result.FailedImports++;
                        result.TotalRows++;
                        continue;
                    }

                    // Crear nuevo LocationDetails (sin verificar duplicados)
                    var locationDetails = new LocationDetailsEntity
                    {
                        TypeStorageSystem = "ESTANTERIA",
                        Section = nivelStr, // Ahora Section es el Nivel (A, B, C, etc.)
                        VerticalLevel = 0, // Siempre 0
                        IdArea = 4, // Fijo según especificación
                        IdStorageStructure = storageStructure.IdStorageStructure
                    };

                    await _locationDetailsRepository.CreateAsync(locationDetails);

                    result.SuccessfulImports++;
                    result.CreatedRecords.Add($"Producto: {descripcion ?? codigo} | Ubicación: {ubicacion} | Nivel: {nivelStr}");
                    result.TotalRows++;
                }
                catch (Exception ex)
                {
                    var errorMessage = ex.InnerException != null
                        ? $"{ex.Message} - Inner: {ex.InnerException.Message}"
                        : ex.Message;

                    result.Errors.Add($"Fila {row}: Error al procesar - {errorMessage}");
                    result.FailedImports++;
                    result.TotalRows++;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Convierte una letra de nivel a su equivalente numérico (A=1, B=2, etc.)
    /// </summary>
    private int ConvertLevelToNumber(string level)
    {
        if (string.IsNullOrWhiteSpace(level))
            return 0;

        level = level.Trim().ToUpper();

        // Si es un solo carácter letra
        if (level.Length == 1 && char.IsLetter(level[0]))
        {
            return level[0] - 'A' + 1; // A=1, B=2, C=3, etc.
        }

        // Si es un número directo
        if (int.TryParse(level, out int numericLevel))
        {
            return numericLevel;
        }

        return 0;
    }

    /// <summary>
    /// Lee un valor double de una celda de Excel, retorna 0 si hay error
    /// </summary>
    private double ReadDoubleValue(IXLCell cell)
    {
        try
        {
            var value = cell.GetValue<string>()?.Trim();
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            if (double.TryParse(value, out double result))
                return result;

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Lee un valor int de una celda de Excel, retorna 0 si hay error
    /// </summary>
    private int ReadIntValue(IXLCell cell)
    {
        try
        {
            var value = cell.GetValue<string>()?.Trim();
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            if (int.TryParse(value, out int result))
                return result;

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Crea un nuevo registro de inventario
    /// </summary>
    public async Task<InventoryResponseDto> CreateInventoryAsync(CreateInventoryDto createDto)
    {
        var inventory = new InventoryEntity
        {
            Stock = createDto.Stock,
            StockMin = createDto.StockMin,
            StockMax = createDto.StockMax,
            LocationDetail = createDto.LocationDetail,
            IdPhysicalInventory = createDto.IdPhysicalInventory,
            IdProduct = createDto.IdProduct,
            IdWarehouse = createDto.IdWarehouse
        };

        var created = await _inventoryRepository.CreateAsync(inventory);

        // Cargar relaciones para la respuesta
        var inventoryWithRelations = await _inventoryRepository.GetByIdAsync(created.IdInventory);

        return MapToResponseDto(inventoryWithRelations!);
    }

    /// <summary>
    /// Obtiene el inventario por ID
    /// </summary>
    public async Task<InventoryResponseDto?> GetInventoryByIdAsync(int idInventory)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(idInventory);
        return inventory == null ? null : MapToResponseDto(inventory);
    }

    /// <summary>
    /// Obtiene todo el inventario con filtros opcionales y paginación
    /// </summary>
    public async Task<PaginatedInventoryResponseDto> GetAllInventoryAsync(
        int pageNumber = 1,
        int pageSize = 20,
        int? idInventory = null,
        int? idWarehouse = null,
        string? warehouseCode = null,
        int? idProduct = null,
        string? productName = null)
    {
        // Validar parámetros de paginación
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 10000) pageSize = 10000; // Límite máximo de 10000 registros por página

        // Obtener el total de registros (solo cuenta, no carga datos)
        var totalCount = await _inventoryRepository.GetInventoryCountAsync(
            idInventory,
            idWarehouse,
            warehouseCode,
            idProduct,
            productName);

        // Calcular el total de páginas
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Obtener los datos paginados directamente desde la base de datos
        var inventories = await _inventoryRepository.GetAllAsync(
            pageNumber,
            pageSize,
            idInventory,
            idWarehouse,
            warehouseCode,
            idProduct,
            productName);

        // Mapear a DTOs
        var items = inventories.Select(MapToResponseDto).ToList();

        // Crear respuesta paginada
        return new PaginatedInventoryResponseDto
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = pageNumber > 1,
            HasNextPage = pageNumber < totalPages
        };
    }

    /// <summary>
    /// Mapea una entidad de inventario a DTO de respuesta
    /// </summary>
    private InventoryResponseDto MapToResponseDto(InventoryEntity inventory)
    {
        return new InventoryResponseDto
        {
            IdInventory = inventory.IdInventory,
            Stock = inventory.Stock,
            StockMin = inventory.StockMin,
            StockMax = inventory.StockMax,
            LocationDetail = inventory.LocationDetail ?? 0,
            IdPhysicalInventory = inventory.IdPhysicalInventory ?? 0,
            IdProduct = inventory.IdProduct,
            ProductName = inventory.Product?.MainName ?? string.Empty,
            ProductCode = inventory.Product?.FirstCode ?? string.Empty,
            Unit = inventory.Product?.Unit ?? string.Empty,
            IdWarehouse = inventory.IdWarehouse,
            WarehouseName = inventory.Warehouse?.Name ?? string.Empty,
            WarehouseCode = inventory.Warehouse?.Code ?? string.Empty
        };
    }
}

/// <summary>
/// Clase auxiliar para agrupar inventarios por Producto + Bodega + Ubicación
/// </summary>
internal class InventoryGroup
{
    public int IdProduct { get; set; }
    public int IdWarehouse { get; set; }
    public int? LocationDetail { get; set; }
    public double? TotalStock { get; set; }
    public string ProductDescription { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
}
