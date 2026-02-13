//ACTUALIZADO A LA VERSION 0.2 DE LA DB
using AVASphere.ApplicationCore.Common.Entities.Products;

namespace AVASphere.ApplicationCore.Inventory.Entities.General;

public class StockMovement
{
    public int IdStockMovement { get; set; }
    public int MovementType { get; set; } // Entrada, Salida, Ajuste, etc.
    public double Quantity { get; set; }
    public double ReferenceType { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int ByUser { get; set; }
    
    // FK
    public int IdProduct { get; set; }
    public Product Product { get; set; } = null!;
    
    public int IdWarehouse { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
}

