namespace AVASphere.ApplicationCore.Inventory.Entities.General;

public class StorageStructure
{
    public int IdStorageStructure { get; set; }
    public string CodeRack { get; set; } = null!;
    public bool OneSection { get; set; }
    public bool HasLevel { get; set; }
    public bool HasSubLevel { get; set; }
    
    // RELACIONES
    public ICollection<LocationDetails> LocationDetails { get; set; } = new List<LocationDetails>();
}