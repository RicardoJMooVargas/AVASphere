using AVASphere.ApplicationCore.Common.Entities.Products;

namespace AVASphere.ApplicationCore.Inventory.Entities.General;

public class PhysicalInventoryDetail
{
    public int IdPhysicalInventoryDetail { get; set; }
    public double SystemQuantity { get; set; }
    public double PhysicalQuantity { get; set; }
    public double Difference { get; set; }
    
    // FK
    public int IdPhysicalInventory { get; set; }
    public PhysicalInventory PhysicalInventory { get; set; } = null!;
    
    public int IdProduct { get; set; }
    public Product Product { get; set; } = null!;
    
    public int? IdLocationDetails { get; set; } // opcional
    public LocationDetails? LocationDetails { get; set; }
}
