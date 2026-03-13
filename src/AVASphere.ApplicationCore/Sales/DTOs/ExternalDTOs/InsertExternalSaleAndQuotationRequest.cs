using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Sales.DTOs;

/// <summary>
/// DTO para la solicitud de insertar una venta desde el sistema externo (InforAVA)
/// y vincularla directamente con una cotización existente.
/// 
/// FLUJO:
/// 1. Obtener datos generales de la venta desde VENTASPORFECHAV
/// 2. Obtener detalles de productos desde DetalleVentaV
/// 3. Registrar la venta en la BD
/// 4. Vincular con la cotización especificada
/// 5. Marcar la cotización como primaria (opcional)
/// </summary>
public class InsertExternalSaleAndQuotationRequest
{
    /// <summary>
    /// Catálogo en el sistema InforAVA (ej: "AVA01", "002").
    /// Identificador de la tienda/sucursal.
    /// </summary>
    public string Catalogo { get; set; } = string.Empty;

    /// <summary>
    /// Número de folio de la venta en InforAVA.
    /// Identificador único de la venta en el sistema externo.
    /// </summary>
    public string Folio { get; set; } = string.Empty;

    /// <summary>
    /// Número de caja del sistema InforAVA.
    /// </summary>
    public string Caja { get; set; } = string.Empty;

    /// <summary>
    /// Serie de la venta en InforAVA.
    /// </summary>
    public string Serie { get; set; } = string.Empty;

    /// <summary>
    /// Número de nota fiscal (si aplica).
    /// </summary>
    public string? NF { get; set; }

    /// <summary>
    /// ID de la cotización existente a vincular con la venta.
    /// </summary>
    public int IdQuotation { get; set; }

    /// <summary>
    /// Indicador si se debe marcar esta cotización como primaria para la venta.
    /// Por defecto: true (la primera vinculación es primaria).
    /// </summary>
    public bool MarkAsPrimary { get; set; } = true;

    /// <summary>
    /// ID del cliente (opcional).
    /// Si se proporciona, se usará para la venta; si no, se buscará desde los datos externos.
    /// </summary>
    public int? IdCustomer { get; set; }

    /// <summary>
    /// Ejecutivo de ventas (opcional).
    /// Si no se proporciona, se usará el agente del sistema externo.
    /// </summary>
    public string? SalesExecutive { get; set; }

    /// <summary>
    /// Comentario adicional sobre la venta (opcional).
    /// </summary>
    public string? Comment { get; set; }
}
