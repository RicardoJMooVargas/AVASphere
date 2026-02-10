using AVASphere.ApplicationCore.Inventory.DTOs;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using AVASphere.ApplicationCore.Common.Interfaces;
using InventoryEntity = AVASphere.ApplicationCore.Inventory.Entities.General.Inventory;
using PhysicalInventoryEntity = AVASphere.ApplicationCore.Inventory.Entities.General.PhysicalInventory;
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
    private readonly MasterDbContext _context;

    public InventoryService(
        IInventoryRepository inventoryRepository,
        IWarehouseRepository warehouseRepository,
        IProductRepository productRepository,
        IPhysicalInventoryRepository physicalInventoryRepository,
        MasterDbContext context)
    {
        _inventoryRepository = inventoryRepository;
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
        _physicalInventoryRepository = physicalInventoryRepository;
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
                        inventoryGroups[groupKey].TotalStock += totalStock;
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
                        existingInventory.Stock = group.TotalStock;
                        existingInventory.LocationDetail = group.LocationDetail;
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
                            Stock = group.TotalStock,
                            StockMin = 0,
                            StockMax = group.TotalStock * 2,
                            LocationDetail = group.LocationDetail,
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
    /// Obtiene todo el inventario con filtros opcionales
    /// </summary>
    public async Task<IEnumerable<InventoryResponseDto>> GetAllInventoryAsync(
        int? idInventory = null,
        int? idWarehouse = null,
        string? warehouseCode = null,
        int? idProduct = null,
        string? productName = null)
    {
        var inventories = await _inventoryRepository.GetAllAsync();
        var query = inventories.AsQueryable();

        // Filtrar por ID de inventario
        if (idInventory.HasValue)
        {
            query = query.Where(i => i.IdInventory == idInventory.Value);
        }

        // Filtrar por ID de bodega
        if (idWarehouse.HasValue)
        {
            query = query.Where(i => i.IdWarehouse == idWarehouse.Value);
        }

        // Filtrar por código de bodega
        if (!string.IsNullOrWhiteSpace(warehouseCode))
        {
            query = query.Where(i => i.Warehouse != null &&
                i.Warehouse.Code.Equals(warehouseCode, StringComparison.OrdinalIgnoreCase));
        }

        // Filtrar por ID de producto
        if (idProduct.HasValue)
        {
            query = query.Where(i => i.IdProduct == idProduct.Value);
        }

        // Buscar por nombre/descripción del producto
        if (!string.IsNullOrWhiteSpace(productName))
        {
            query = query.Where(i => i.Product != null &&
                i.Product.MainName.Contains(productName, StringComparison.OrdinalIgnoreCase));
        }

        return query.Select(MapToResponseDto).ToList();
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
            LocationDetail = inventory.LocationDetail,
            IdPhysicalInventory = inventory.IdPhysicalInventory,
            IdProduct = inventory.IdProduct,
            ProductName = inventory.Product?.MainName ?? string.Empty,
            ProductCode = inventory.Product?.FirstCode ?? string.Empty,
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
    public int LocationDetail { get; set; }
    public double TotalStock { get; set; }
    public string ProductDescription { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
}
