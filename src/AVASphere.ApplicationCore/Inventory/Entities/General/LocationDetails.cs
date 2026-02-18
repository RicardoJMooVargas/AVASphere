using AVASphere.ApplicationCore.Common.Entities.Catalogs;

namespace AVASphere.ApplicationCore.Inventory.Entities.General;

public class LocationDetails
{
    public int IdLocationDetails { get; set; }
    public string TypeStorageSystem { get; set; } = null!;
    public string Section { get; set; } = null!; //A o B
    public int VerticalLevel { get; set; }
    
    // FK
    public int IdArea { get; set; }
    public Area Area { get; set; } = null!;
    
    public int IdStorageStructure { get; set; }
    public StorageStructure StorageStructure { get; set; } = null!;
    
    // RELACIONES
    public ICollection<PhysicalInventoryDetail> PhysicalInventoryDetails { get; set; } = new List<PhysicalInventoryDetail>();
}
