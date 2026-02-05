using AVASphere.ApplicationCore.Common.Entities.Catalogs;

namespace AVASphere.ApplicationCore.Inventory.Entities.General;

public class StorageStructure
{
    public int IdStorageStructure { get; set; }
    public string CodeRack { get; set; } = null!;
    public bool OneSection { get; set; }
    public bool HasLevel { get; set; }
    public bool HasSubLevel { get; set; }
    
    // Relación con Warehouse
    public int IdWarehouse { get; set; }
    
    // Relación con Area (el área donde está ubicado el rack) - nullable
    public int? IdArea { get; set; }
    
    // RELACIONES
    public Warehouse Warehouse { get; set; } = null!;
    public Area? Area { get; set; }
    public ICollection<LocationDetails> LocationDetails { get; set; } = new List<LocationDetails>();
}