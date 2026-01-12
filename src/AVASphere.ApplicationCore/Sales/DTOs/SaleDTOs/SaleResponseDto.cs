namespace AVASphere.ApplicationCore.Sales.DTOs;

/// <summary>
/// DTO de respuesta para la creación de una venta.
/// Contiene la información básica de la venta creada.
/// </summary>
public class SaleResponseDto
{
    /// <summary>
    /// ID de la venta creada.
    /// </summary>
    public int IdSale { get; set; }

    /// <summary>
    /// Folio/Número de la venta.
    /// </summary>
    public string Folio { get; set; } = string.Empty;

    /// <summary>
    /// Monto total de la venta.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Fecha de la venta.
    /// </summary>
    public DateTime SaleDate { get; set; }

    /// <summary>
    /// Tipo de venta (ej: "External", "Internal").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad de cotizaciones vinculadas.
    /// </summary>
    public int LinkedQuotationCount { get; set; }

    /// <summary>
    /// Cantidad de productos en la venta.
    /// </summary>
    public int ProductCount { get; set; }

    /// <summary>
    /// Mensaje de éxito (opcional).
    /// </summary>
    public string? Message { get; set; } = "Sale created successfully and linked to quotation.";
}
