using AVASphere.ApplicationCore.Common.Entities.General;

namespace AVASphere.ApplicationCore.Common.Entities;

// CATALOG AREA
public class Area
{
    public int IdArea { get; set; }
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }

    // RELACIONES
    public ICollection<Rol> Rol { get; set; } = new List<Rol>();
}