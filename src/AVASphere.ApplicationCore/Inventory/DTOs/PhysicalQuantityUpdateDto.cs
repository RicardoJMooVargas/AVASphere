using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Inventory.DTOs;

/// <summary>
/// DTO para actualizar la cantidad física en el inventario
/// </summary>
public class PhysicalQuantityUpdateDto
{
    [Required(ErrorMessage = "El ID del detalle de inventario físico es requerido")]
    public int IdPhysicalInventoryDetail { get; set; }
    
    [Required(ErrorMessage = "La cantidad física es requerida")]
    [Range(0, double.MaxValue, ErrorMessage = "La cantidad física debe ser mayor o igual a 0")]
    public double PhysicalQuantity { get; set; }
    
    /// <summary>
    /// Observaciones opcionales sobre el conteo
    /// </summary>
    public string? Observations { get; set; }
}

/// <summary>
/// DTO para crear un nuevo registro de inventario físico con cantidad
/// </summary>
public class PhysicalInventoryDetailCreateDto
{
    [Required(ErrorMessage = "El ID del inventario físico es requerido")]
    public int IdPhysicalInventory { get; set; }
    
    [Required(ErrorMessage = "El ID del producto es requerido")]
    public int IdProduct { get; set; }
    
    [Required(ErrorMessage = "La cantidad del sistema es requerida")]
    [Range(0, double.MaxValue, ErrorMessage = "La cantidad del sistema debe ser mayor o igual a 0")]
    public double SystemQuantity { get; set; }
    
    [Required(ErrorMessage = "La cantidad física es requerida")]
    [Range(0, double.MaxValue, ErrorMessage = "La cantidad física debe ser mayor o igual a 0")]
    public double PhysicalQuantity { get; set; }
    
    /// <summary>
    /// ID opcional de la ubicación específica
    /// </summary>
    public int? IdLocationDetails { get; set; }
    
    /// <summary>
    /// Observaciones opcionales sobre el conteo
    /// </summary>
    public string? Observations { get; set; }
}

/// <summary>
/// DTO de respuesta con información completa del detalle
/// </summary>
public class PhysicalInventoryDetailResponseDto
{
    public int IdPhysicalInventoryDetail { get; set; }
    public double SystemQuantity { get; set; }
    public double PhysicalQuantity { get; set; }
    public double Difference { get; set; }
    
    // Información del inventario
    public int IdPhysicalInventory { get; set; }
    public string PhysicalInventoryName { get; set; } = string.Empty;
    
    // Información del producto
    public int IdProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    
    // Información de ubicación (opcional)
    public int? IdLocationDetails { get; set; }
    public string? LocationName { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
