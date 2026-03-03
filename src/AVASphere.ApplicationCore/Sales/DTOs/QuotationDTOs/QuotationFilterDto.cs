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
    /// Fecha de inicio para filtrar por rango (por defecto: primer día del mes actual).
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Fecha de fin para filtrar por rango (por defecto: último día del mes actual).
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Constructor que establece valores por defecto.
    /// </summary>
    public QuotationFilterDto()
    {
        var today = DateTime.UtcNow;
        StartDate = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        EndDate = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month), 23, 59, 59, DateTimeKind.Utc);
    }
}
