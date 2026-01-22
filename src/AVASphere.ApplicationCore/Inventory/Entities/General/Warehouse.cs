namespace AVASphere.ApplicationCore.Inventory.Entities.General;

public class Warehouse
{
    public int IdWarehouse { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Location { get; set; }
    public double IsMain { get; set; }
    public double Active { get; set; }
    
    // RELACIONES
    public ICollection<PhysicalInventory> PhysicalInventories { get; set; } = new List<PhysicalInventory>();
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}
