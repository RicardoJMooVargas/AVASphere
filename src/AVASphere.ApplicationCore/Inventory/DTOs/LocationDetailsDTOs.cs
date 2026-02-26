using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Inventory.DTOs;

/// <summary>
/// DTO para crear o actualizar detalles de ubicación en el sistema de inventario.
/// Permite asociar una ubicación específica con registros de inventario físico o inventario general.
/// </summary>
public class LocationDetailsRequestDto
{
    /// <summary>
    /// Sección de la ubicación (ej: "A", "B", "C").
    /// Identifica la división horizontal dentro de la estructura de almacenamiento.
    /// </summary>
    [Required(ErrorMessage = "La sección es requerida")]
    [StringLength(100, ErrorMessage = "La sección no puede exceder 100 caracteres")]
    public string Section { get; set; } = null!;
    
    /// <summary>
    /// Nivel vertical de la ubicación (ej: 1, 2, 3).
    /// Indica la altura o piso dentro de la estructura de almacenamiento.
    /// Debe ser mayor a 0.
    /// </summary>
    [Required(ErrorMessage = "El nivel vertical es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El nivel vertical debe ser mayor a 0")]
    public int VerticalLevel { get; set; }
    
    /// <summary>
    /// ID del área donde se encuentra la ubicación.
    /// Opcional: Si no se proporciona, se tomará del área del usuario autenticado.
    /// </summary>
    public int? IdArea { get; set; } // Opcional, si no se proporciona se toma del usuario
    
    /// <summary>
    /// ID de la estructura de almacenamiento (rack, estantería, etc.).
    /// Define el tipo y características físicas del sistema de almacenamiento.
    /// Requerido para crear la ubicación.
    /// </summary>
    [Required(ErrorMessage = "El ID de la estructura de almacenamiento es requerido")]
    public int IdStorageStructure { get; set; }
    
    /// <summary>
    /// ID del registro de inventario general a asociar con esta ubicación.
    /// Opcional: Se usa cuando se quiere asignar una ubicación a un producto en el inventario general.
    /// Si se proporciona, se actualizará el campo LocationDetail de la tabla Inventory.
    /// </summary>
    public int? IdInventory { get; set; }
    
    /// <summary>
    /// ID específico del detalle de inventario físico a asociar con esta ubicación.
    /// Opcional: Se usa cuando se quiere asignar una ubicación a un producto específico 
    /// durante un conteo de inventario físico. Permite vincular directamente el detalle
    /// del conteo con su ubicación física.
    /// </summary>
    public int? IdPhysicalInventoryDetail { get; set; } // ID específico del detalle de inventario físico
    
    // Campo IdProduct comentado - no se utiliza actualmente en la lógica de negocio
    // public int? IdProduct { get; set; }
}

/// <summary>
/// DTO para actualizar detalles de ubicación existentes.
/// Contiene todos los campos requeridos para modificar una ubicación,
/// a diferencia del RequestDto, aquí todos los campos de ubicación son obligatorios.
/// </summary>
public class LocationDetailsUpdateDto
{
    /// <summary>
    /// Nueva sección de la ubicación (ej: "A", "B", "C").
    /// Requerido para la actualización.
    /// </summary>
    [Required(ErrorMessage = "La sección es requerida")]
    [StringLength(100, ErrorMessage = "La sección no puede exceder 100 caracteres")]
    public string Section { get; set; } = null!;
    
    /// <summary>
    /// Nuevo nivel vertical de la ubicación.
    /// Requerido y debe ser mayor a 0.
    /// </summary>
    [Required(ErrorMessage = "El nivel vertical es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El nivel vertical debe ser mayor a 0")]
    public int VerticalLevel { get; set; }
    
    /// <summary>
    /// ID del área donde se moverá la ubicación.
    /// Requerido para la actualización - permite cambiar la ubicación de área.
    /// </summary>
    [Required(ErrorMessage = "El ID del área es requerido")]
    public int IdArea { get; set; }
    
    /// <summary>
    /// ID de la nueva estructura de almacenamiento.
    /// Requerido - permite cambiar el tipo de estructura de almacenamiento.
    /// </summary>
    [Required(ErrorMessage = "El ID de la estructura de almacenamiento es requerido")]
    public int IdStorageStructure { get; set; }
}

/// <summary>
/// DTO de respuesta que contiene toda la información de una ubicación.
/// Incluye datos relacionados de área y estructura de almacenamiento para proporcionar
/// información completa sin necesidad de consultas adicionales.
/// </summary>
public class LocationDetailsResponseDto
{
    /// <summary>
    /// Identificador único de la ubicación.
    /// </summary>
    public int IdLocationDetails { get; set; }
    
    /// <summary>
    /// Sección de la ubicación (ej: "A", "B", "C").
    /// </summary>
    public string Section { get; set; } = null!;
    
    /// <summary>
    /// Nivel vertical de la ubicación (ej: 1, 2, 3).
    /// </summary>
    public int VerticalLevel { get; set; }
    
    /// <summary>
    /// ID del área a la que pertenece la ubicación.
    /// </summary>
    public int IdArea { get; set; }
    
    /// <summary>
    /// Nombre del área (información relacionada para facilitar la visualización).
    /// </summary>
    public string? AreaName { get; set; }
    
    /// <summary>
    /// Nombre normalizado del área (en mayúsculas, sin espacios).
    /// </summary>
    public string? AreaNormalizedName { get; set; }
    
    /// <summary>
    /// ID de la estructura de almacenamiento asociada.
    /// </summary>
    public int IdStorageStructure { get; set; }
    
    /// <summary>
    /// Código del rack o estructura de almacenamiento (información relacionada).
    /// </summary>
    public string? CodeRack { get; set; }
    
    /// <summary>
    /// Tipo de sistema de almacenamiento (ej: "Rack Cantilever", "Estantería").
    /// Información obtenida de la estructura de almacenamiento asociada.
    /// </summary>
    public string? TypeStorageSystem { get; set; } // Viene de StorageStructure
}
