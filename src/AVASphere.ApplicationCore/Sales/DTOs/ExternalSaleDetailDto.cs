namespace AVASphere.ApplicationCore.Sales.DTOs;

/// <summary>
/// DTO para representar el detalle de un producto/movimiento en una venta del sistema externo InforAVA.
/// 
/// PROPÓSITO:
/// Mapear la respuesta de detalles de venta de la API externa (DetalleVentaV) a un objeto
/// fuertemente tipado que pueda ser usado en la aplicación.
/// 
/// ESTRUCTURA:
/// Los datos provienen del endpoint: DetalleVentaV?CATALOGO={cat}&NF={nf}&CAJA={caja}&SERIE={serie}&FOLIO={folio}
/// que retorna un arreglo de productos/movimientos de una venta específica.
/// </summary>
public class ExternalSaleDetailDto
{
    /// <summary>Número secuencial del movimiento.</summary>
    public int Mov { get; set; }

    /// <summary>Código del producto.</summary>
    public string? Codigo { get; set; }

    /// <summary>Descripción del producto.</summary>
    public string? Descripcion { get; set; }

    /// <summary>Unidad de medida del producto.</summary>
    public string? Unidad { get; set; }

    /// <summary>Cantidad vendida.</summary>
    public decimal Cantidad { get; set; }

    /// <summary>Tipo de precio.</summary>
    public int Tprc { get; set; }

    /// <summary>Precio unitario del producto.</summary>
    public decimal Precio { get; set; }

    /// <summary>Descuento aplicado.</summary>
    public decimal Dcto { get; set; }

    /// <summary>Impuesto.</summary>
    public decimal Impto { get; set; }

    /// <summary>Importe (subtotal sin descuento ni impuesto).</summary>
    public decimal Importe { get; set; }

    /// <summary>Descuento total aplicado.</summary>
    public decimal Descuento { get; set; }

    /// <summary>Impuesto total.</summary>
    public decimal Impuesto { get; set; }

    /// <summary>Total incluyendo descuentos e impuestos.</summary>
    public decimal Total { get; set; }
}
