using AVASphere.ApplicationCore.Common.Entities.Products;

namespace AVASphere.ApplicationCore.Inventory.Entities.General;

public class PhysicalInventory
{
    public int IdPhysicalInventory { get; set; }
    public DateTime InventoryDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = null!; // Open, Closed, Cancelled
    public int CreatedBy { get; set; }
    public string? Observations { get; set; }
    
    // FK
    public int IdWarehouse { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    
    // RELACIONES
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public ICollection<PhysicalInventoryDetail> PhysicalInventoryDetails { get; set; } = new List<PhysicalInventoryDetail>();
}
