using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Projects.Enum;

namespace AVASphere.ApplicationCore.Inventory.Entities.General;

public class PhysicalInventoryDetail
{
    public int IdPhysicalInventoryDetail { get; set; }
    public double SystemQuantity { get; set; }
    public double PhysicalQuantity { get; set; }
    public double Difference { get; set; }
    public StatusInventoryProduct? StatusInventoryProduct { get; set; } // Opcional
    
    // FK
    public int IdPhysicalInventory { get; set; }
    public PhysicalInventory PhysicalInventory { get; set; } = null!;
    
    public int IdProduct { get; set; }
    public Product Product { get; set; } = null!;
    
    public int? IdLocationDetails { get; set; } // opcional
    public LocationDetails? LocationDetails { get; set; }
}
