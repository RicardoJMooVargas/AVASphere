namespace AVASphere.ApplicationCore.Sales.DTOs;

/// <summary>
/// DTO para crear nuevos seguimientos en una cotización.
/// NO incluye el campo Id porque se genera automáticamente en el servidor.
/// </summary>
public class QuotationFollowupCreateDto
{
    /// <summary>
    /// Fecha del seguimiento (opcional, por defecto se usa la fecha actual).
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// Comentario del seguimiento (obligatorio).
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// ID del usuario que crea el seguimiento (opcional, se usa el usuario autenticado si no se especifica).
    /// </summary>
    public string? UserId { get; set; }
}

