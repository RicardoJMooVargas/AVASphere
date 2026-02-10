using AVASphere.ApplicationCore.Common.Entities.Products;

namespace AVASphere.ApplicationCore.Inventory.Entities.General;

public class Inventory
{
    public int IdInventory { get; set; }
    public double Stock { get; set; }
    public double StockMin { get; set; }
    public double StockMax { get; set; }
    public double? LocationDetail { get; set; } // Opcional
    
    // FK
    public int? IdPhysicalInventory { get; set; } // Opcional
    public PhysicalInventory? PhysicalInventory { get; set; }
    
    public int IdProduct { get; set; }
    public Product Product { get; set; } = null!;
    
    public int IdWarehouse { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
}
