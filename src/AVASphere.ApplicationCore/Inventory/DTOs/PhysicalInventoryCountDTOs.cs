using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Inventory.DTOs;

/// <summary>
/// DTO para crear o actualizar un conteo específico de un producto
/// </summary>
public class CreateOrUpdatePhysicalCountDto
{
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Physical quantity must be greater than or equal to 0")]
    public double PhysicalQuantity { get; set; }
    
    [Required]
    public int IdPhysicalInventory { get; set; }
    
    [Required]
    public int IdProduct { get; set; }
    
    public int? IdLocationDetails { get; set; }
}

/// <summary>
/// DTO de respuesta para un conteo de inventario físico
/// </summary>
public class PhysicalInventoryCountResponseDto
{
    public int IdPhysicalInventoryDetail { get; set; }
    public double SystemQuantity { get; set; }
    public double PhysicalQuantity { get; set; }
    public double Difference { get; set; }
    public int IdPhysicalInventory { get; set; }
    public int IdProduct { get; set; }
    public string ProductMainName { get; set; } = string.Empty;
    public int? IdLocationDetails { get; set; }
    public string LocationDetailsCode { get; set; } = string.Empty;
    public bool IsNewRecord { get; set; } // Indica si se creó un nuevo registro
}

/// <summary>
/// DTO para filtros de selección de productos al crear un inventario físico
/// </summary>
public class ProductSelectionFilterDto
{
    /// <summary>
    /// ID del proveedor a filtrar (opcional)
    /// </summary>
    public int? IdSupplier { get; set; }
    
    /// <summary>
    /// Nombre del proveedor a filtrar (opcional)
    /// </summary>
    public string? SupplierName { get; set; }
    
    /// <summary>
    /// Filtros por propiedades específicas
    /// Key: Nombre de la propiedad ("Familia", "Clase", "Línea")
    /// Value: Valor de la propiedad
    /// </summary>
    public Dictionary<string, string>? ProductProperties { get; set; }
}
