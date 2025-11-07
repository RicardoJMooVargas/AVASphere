namespace AVASphere.ApplicationCore.Common.Entities.Jsons;

public class SingleProductJson
{
    public int? ProductId { get; set; } // Opcional: referencia al producto real
    public double Quantity { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }  
    public decimal TotalPrice { get; set; }
    public string Unit { get; set; } = string.Empty;
}