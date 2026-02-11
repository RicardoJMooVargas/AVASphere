namespace AVASphere.ApplicationCore.Inventory.DTOs;

/// <summary>
/// DTO para importar inventario desde Excel
/// </summary>
public class ImportInventoryDto
{
    /// <summary>
    /// Código del producto (debe coincidir con CodeJson.Code en Products)
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del producto (solo para referencia, no se importa)
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Unidad del producto (solo para referencia, no se importa)
    /// </summary>
    public string Unidad { get; set; } = string.Empty;

    /// <summary>
    /// Estado activo (solo para referencia, no se importa)
    /// </summary>
    public bool Activo { get; set; }

    /// <summary>
    /// Stock en bodega AVA01
    /// </summary>
    public double AVA01 { get; set; }

    /// <summary>
    /// Stock en bodega AVA02
    /// </summary>
    public double AVA02 { get; set; }

    /// <summary>
    /// Stock en bodega AVA03
    /// </summary>
    public double AVA03 { get; set; }

    /// <summary>
    /// Stock en bodega AVA04
    /// </summary>
    public double AVA04 { get; set; }

    /// <summary>
    /// Proveedor (solo para referencia, no se importa)
    /// </summary>
    public string Proveedor { get; set; } = string.Empty;

    /// <summary>
    /// Familia (solo para referencia, no se importa)
    /// </summary>
    public string Familia { get; set; } = string.Empty;

    /// <summary>
    /// Clase (solo para referencia, no se importa)
    /// </summary>
    public string Clase { get; set; } = string.Empty;

    /// <summary>
    /// Línea (solo para referencia, no se importa)
    /// </summary>
    public string Linea { get; set; } = string.Empty;

    /// <summary>
    /// Ubicación (código de bodega donde se guardará)
    /// </summary>
    public string Ubicacion { get; set; } = string.Empty;
}

/// <summary>
/// Resultado de la importación de inventario
/// </summary>
public class ImportInventoryResultDto
{
    /// <summary>
    /// Total de filas procesadas en el Excel
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    /// Total de registros de inventario creados exitosamente
    /// </summary>
    public int SuccessfulImports { get; set; }

    /// <summary>
    /// Total de registros que fallaron
    /// </summary>
    public int FailedImports { get; set; }

    /// <summary>
    /// Total de productos no encontrados
    /// </summary>
    public int ProductsNotFound { get; set; }

    /// <summary>
    /// Total de bodegas no encontradas
    /// </summary>
    public int WarehousesNotFound { get; set; }

    /// <summary>
    /// Lista de errores ocurridos durante la importación
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Lista de advertencias (registros omitidos sin error crítico)
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Detalles de registros creados
    /// </summary>
    public List<string> CreatedRecords { get; set; } = new();
}

/// <summary>
/// DTO para crear un registro de inventario
/// </summary>
public class CreateInventoryDto
{
    public double Stock { get; set; }
    public double StockMin { get; set; }
    public double StockMax { get; set; }
    public double LocationDetail { get; set; }
    public int IdPhysicalInventory { get; set; }
    public int IdProduct { get; set; }
    public int IdWarehouse { get; set; }
}

/// <summary>
/// DTO para respuesta de inventario
/// </summary>
public class InventoryResponseDto
{
    public int IdInventory { get; set; }
    public double Stock { get; set; }
    public double StockMin { get; set; }
    public double StockMax { get; set; }
    public double LocationDetail { get; set; }
    public int IdPhysicalInventory { get; set; }
    public int IdProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public int IdWarehouse { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
}
