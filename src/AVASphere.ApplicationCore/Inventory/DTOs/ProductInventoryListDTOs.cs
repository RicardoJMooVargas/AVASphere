namespace AVASphere.ApplicationCore.Inventory.DTOs;

/// <summary>
/// DTO para respuesta de lista de productos para conteo físico
/// </summary>
public class ProductInventoryListDto
{
    public int? IdPhysicalInventoryDetail { get; set; } // Puede ser null si proviene de productos directos
    public double SystemQuantity { get; set; }
    public double PhysicalQuantity { get; set; }
    public double Difference { get; set; }
    public string ProductMainName { get; set; } = string.Empty;
    public string ProductUnit { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public string ProductSupplierName { get; set; } = string.Empty;
    public string ProductCodeJsonCode { get; set; } = string.Empty; // Type: Principal
    public List<ProductPropertyDto> ProductProperties { get; set; } = new(); // All JSON properties
    public int? IdLocationDetails { get; set; } // Puede ser null
    public string LocationDetailsCode { get; set; } = string.Empty; // Compuesto de CodeRack + Section + VerticalLevel
    
    // Campo para identificar si viene de productos directos
    public bool IsDirectFromProducts { get; set; }
}

/// <summary>
/// DTO para propiedades de producto
/// </summary>
public class ProductPropertyDto
{
    public int IdProductProperties { get; set; }
    public string? CustomValue { get; set; }
    public int IdPropertyValue { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public string PropertyValue { get; set; } = string.Empty;
}

/// <summary>
/// DTO para respuesta completa de la lista de productos
/// </summary>
public class ProductInventoryListResponseDto
{
    public List<ProductInventoryListDto> Products { get; set; } = new();
    public int TotalProducts { get; set; }
    public bool HasInventoryRecords { get; set; } // True si hay registros en Inventory, false si se obtuvieron directamente de productos
    public string WarehouseName { get; set; } = string.Empty;
    public string UserAreaName { get; set; } = string.Empty;
}
