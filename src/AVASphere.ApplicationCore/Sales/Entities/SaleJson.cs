
namespace AVASphere.ApplicationCore.Common.Entities.Jsons;

public class SaleJson
{
    public string NF { get; set; } = string.Empty;
    public string Caja { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Folio { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Hora { get; set; } = string.Empty;
    public string ZN { get; set; } = string.Empty;
    public string Agente { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public string TelCliente { get; set; } = string.Empty;
    public string EmailCliente { get; set; } = string.Empty;
    public string DireccionCliente { get; set; } = string.Empty;
    public string PoblacionCliente { get; set; } = string.Empty;
    public int Movs { get; set; }
    public decimal Importe { get; set; }
    public decimal Descuento { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }

    public List<SingleProductJson> Products { get; set; } = new List<SingleProductJson>();

}
public class VentasResponse
{
    public List<SaleJson> Ventas { get; set; } = new();
}

