namespace AVASphere.ApplicationCore.Common.Entities.Jsons;

public class SingleProductJson
{
    public int? ProductId { get; set; } // Opcional: referencia al producto real
    public double Quantity { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Unit { get; set; } = string.Empty;

    // (Opcional) Si quieres almacenar subtotal y tax por línea, puedes añadir:
    // public decimal LineSubtotal { get; set; }
    // public decimal LineTax { get; set; }

    // Recalcula TotalPrice a partir de Quantity y UnitPrice.
    /* Si pasas taxRate (por ejemplo 0.16m para 16%) lo aplicará sobre el subtotal.
    public void RecalculateLineTotals(decimal taxRate = 0m)
    {
        var subtotal = decimal.Round(Quantity * UnitPrice, 2, MidpointRounding.AwayFromZero);
        var tax = decimal.Round(subtotal * taxRate, 2, MidpointRounding.AwayFromZero);
        TotalPrice = subtotal + tax;
    }*/
}