using AVASphere.ApplicationCore.Sales.Entities;

namespace AVASphere.ApplicationCore.Sales.DTOs;

/// <summary>
/// DTO que representa una venta externa del sistema InforAVA.
/// Se utiliza para mapear los datos devueltos por la API externa:
/// http://apivaa.ddns.net:8080/api/rest/tsm/VENTASPORFECHAV?CATALOGO=AVA01&FECHA=YYYY-MM-DD
/// 
/// MOTIVO: Separar la estructura de datos externa de nuestra lógica interna.
/// Si la API externa cambia, solo actualizamos este DTO, no toda la aplicación.
/// </summary>
public class ExternalSalesDto
{
    /// <summary>
    /// Indicador NF (Nota Fiscal).
    /// ejemplo: "F" para factura y N para notas.
    /// </summary>
    public string? NF { get; set; }

    /// <summary>
    /// Número de caja.
    /// 13 para caja registradora especifica de venta.
    /// </summary>
    public string? Caja { get; set; }

    /// <summary>
    /// Serie de la venta.
    /// ejemplo: "CR".
    /// </summary>
    public string? Serie { get; set; }

    /// <summary>
    /// Folio/Número de la venta en el sistema InforAVA.
    /// Este es el identificador principal.
    /// </summary>
    public string? Folio { get; set; }

    /// <summary>
    /// Fecha de la venta (formato: YYYY-MM-DD).
    /// </summary>
    public string? Fecha { get; set; }

    /// <summary>
    /// Hora de la venta (formato: HH:MM:SS).
    /// </summary>
    public string? Hora { get; set; }

    /// <summary>
    /// Zona de venta (ej: "1 Local").
    /// </summary>
    public string? ZN { get; set; }

    /// <summary>
    /// Agente que realizó la venta.
    /// </summary>
    public string? Agente { get; set; }

    /// <summary>
    /// Código del cliente en el sistema externo.
    /// </summary>
    public string? Cliente { get; set; }

    /// <summary>
    /// Nombre completo del cliente.
    /// </summary>
    public string? NombreCliente { get; set; }

    /// <summary>
    /// Teléfono del cliente.
    /// </summary>
    public string? TelCliente { get; set; }

    /// <summary>
    /// Email del cliente.
    /// </summary>
    public string? EmailCliente { get; set; }

    /// <summary>
    /// Dirección del cliente.
    /// </summary>
    public string? DireccionCliente { get; set; }

    /// <summary>
    /// Población/Código postal del cliente.
    /// </summary>
    public string? PoblacionCliente { get; set; }

    /// <summary>
    /// Cantidad de movimientos/líneas en la venta.
    /// </summary>
    public int Movs { get; set; }

    /// <summary>
    /// Importe subtotal (antes de descuentos e impuestos).
    /// </summary>
    public decimal Importe { get; set; }

    /// <summary>
    /// Descuento aplicado.
    /// </summary>
    public decimal Descuento { get; set; }

    /// <summary>
    /// Impuesto/IVA.
    /// </summary>
    public decimal Impuesto { get; set; }

    /// <summary>
    /// Total final de la venta (Importe - Descuento + Impuesto).
    /// </summary>
    public decimal Total { get; set; }
}

/// <summary>
/// DTO que combina información de ventas externas (InforAVA) 
/// con información interna del sistema AVASphere.
/// 
/// MOTIVO: Presentar una vista consolidada de la venta que incluya
/// tanto datos del sistema externo como información local enriquecida.
/// 
/// CONTENIDO:
/// - Datos originales de InforAVA (siempre presentes)
/// - Datos adicionales internos si la venta existe (null si no existe)
/// </summary>
public class CombinedSalesDto
{
    /// <summary>
    /// Datos de la venta del sistema externo InforAVA.
    /// Este campo siempre contiene datos.
    /// </summary>
    public ExternalSalesDto? ExternalSales { get; set; }

    /// <summary>
    /// Datos de la venta registrada internamente en AVASphere.
    /// Será null si la venta no existe internamente.
    /// </summary>
    public Sale? InternalSales { get; set; }

    /// <summary>
    /// Indica si la venta externa fue encontrada en el sistema interno.
    /// MOTIVO: Permite identificar ventas que existen en InforAVA pero 
    /// aún no han sido importadas o registradas internamente.
    /// </summary>
    public bool IsLinked { get; set; }

    /// <summary>
    /// Mensaje de estado que indica por qué la venta está o no vinculada.
    /// </summary>
    public string? StatusMessage { get; set; }

    // ========== CAMPOS ADICIONALES INTERNOS (null si IsLinked = false) ==========

    /// <summary>
    /// Nivel de satisfacción (0-5) registrado después de la venta.
    /// Solo disponible si la venta existe internamente (IsLinked = true).
    /// Será null si la venta no existe internamente.
    /// </summary>
    public int? SatisfactionLevel { get; set; }

    /// <summary>
    /// Razón de la satisfacción/insatisfacción.
    /// Solo disponible si la venta existe internamente (IsLinked = true).
    /// Será null si la venta no existe internamente.
    /// </summary>
    public string? SatisfactionReason { get; set; }

    /// <summary>
    /// Comentario adicional sobre la venta.
    /// Solo disponible si la venta existe internamente (IsLinked = true).
    /// Será null si la venta no existe internamente.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Fecha programada para seguimiento post-venta.
    /// Solo disponible si la venta existe internamente (IsLinked = true).
    /// Será null si la venta no existe internamente.
    /// </summary>
    public DateTime? AfterSalesFollowupDate { get; set; }

    /// <summary>
    /// Lista de referencias a cotizaciones vinculadas a esta venta.
    /// Solo disponible si la venta existe internamente (IsLinked = true).
    /// Será null o lista vacía si la venta no existe internamente.
    /// 
    /// Estructura de cada elemento: 
    /// { "IdQuotation": 123, "QuotationFolio": "Q00456", ... }
    /// </summary>
    public List<QuotationReference>? LinkedQuotations { get; set; }
}
