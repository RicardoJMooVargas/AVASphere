using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Sales.DTOs.SaleDTOs;

/// <summary>
/// DTO para crear una venta y vincularla inmediatamente con una cotización.
/// Usado cuando desde el frontend se selecciona una cotización para convertirla en venta.
/// </summary>
public class CreateSaleWithQuotationLinkDto
{
    /// <summary>
    /// ID de la cotización a vincular con la venta
    /// </summary>
    public int IdQuotation { get; set; }

    /// <summary>
    /// ID del cliente
    /// </summary>
    public int IdCustomer { get; set; }

    /// <summary>
    /// Ejecutivo de ventas responsable
    /// </summary>QuotationLinkDto QuotationLink 
    public string SalesExecutive { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de la venta (opcional, por defecto DateTime.UtcNow)
    /// </summary>
    public DateTime? SaleDate { get; set; }

    /// <summary>
    /// Tipo de venta (ej: "Internal", "Quotation", "Direct")
    /// </summary>
    public string Type { get; set; } = "Quotation";

    /// <summary>
    /// Folio de la venta
    /// </summary>
    public string Folio { get; set; } = string.Empty;

    /// <summary>
    /// Monto total de la venta
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Conductor de entrega (opcional)
    /// </summary>
    public string? DeliveryDriver { get; set; }

    /// <summary>
    /// ¿Entrega a domicilio?
    /// </summary>
    public bool HomeDelivery { get; set; } = false;

    /// <summary>
    /// Fecha de entrega (opcional)
    /// </summary>
    public DateTime? DeliveryDate { get; set; }

    /// <summary>
    /// Nivel de satisfacción (0-5)
    /// </summary>
    public int? SatisfactionLevel { get; set; } = 0;

    /// <summary>
    /// Razón de la satisfacción
    /// </summary>
    public string? SatisfactionReason { get; set; }

    /// <summary>
    /// Comentarios adicionales
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Fecha de seguimiento post-venta (opcional)
    /// </summary>
    public DateTime? AfterSalesFollowupDate { get; set; }

    /// <summary>
    /// ID de configuración del sistema
    /// </summary>
    public int IdConfigSys { get; set; } = 1;

    /// <summary>
    /// Comentario general para la relación SaleQuotation
    /// </summary>
    public string? GeneralCommentForLink { get; set; }

    /// <summary>
    /// Marcar la cotización como primaria para esta venta
    /// </summary>
    public bool MarkAsPrimary { get; set; } = true;
}
