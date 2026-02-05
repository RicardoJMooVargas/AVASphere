using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Inventory.Entities.General;

namespace AVASphere.ApplicationCore.Common.Entities.Catalogs;

// CATALOG AREA
public class Area
{
    public int IdArea { get; set; }
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }

    // RELACIONES
    public ICollection<Rol> Rol { get; set; } = new List<Rol>();
    public ICollection<StorageStructure> StorageStructures { get; set; } = new List<StorageStructure>();
}