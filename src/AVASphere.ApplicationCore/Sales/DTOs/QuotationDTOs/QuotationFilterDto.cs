namespace AVASphere.ApplicationCore.Sales.DTOs;

/// <summary>
/// DTO para filtrar cotizaciones con opciones de búsqueda flexibles.
/// </summary>
public class QuotationFilterDto
{
    /// <summary>
    /// Filtro por ID de cotización específica (opcional).
    /// </summary>
    public int? IdQuotation { get; set; }

    /// <summary>
    /// Filtro por folio de la cotización (opcional).
    /// </summary>
    public int? Folio { get; set; }

    /// <summary>
    /// Filtro por ID de cliente (opcional).
    /// </summary>
    public int? IdCustomer { get; set; }

    /// <summary>
    /// Filtro por nombre de cliente (opcional).
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Filtro por ID externo (opcional).
    /// </summary>
    public int? ExternalId { get; set; } = 0;

    /// <summary>
    /// Filtro por nombre de usuario en SalesExecutives (opcional).
    /// Si se proporciona, solo devuelve cotizaciones que contengan este usuario en la lista SalesExecutives.
    /// EXCEPCIÓN: Si el nombre de usuario es "Admin", "admin", "Administrador" o "administrador", 
    /// se devuelven TODAS las cotizaciones sin aplicar este filtro.
    /// </summary>
    public string? SalesExecutive { get; set; }

    /// <summary>
    /// Fecha de inicio del rango a consultar (opcional, formato YYYY-MM-DD).
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Fecha de fin del rango a consultar (opcional, formato YYYY-MM-DD).
    /// </summary>
    public DateTime? EndDate { get; set; }
}
