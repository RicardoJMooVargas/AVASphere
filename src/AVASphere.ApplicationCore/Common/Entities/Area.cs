namespace AVASphere.ApplicationCore.Common.Entities;


public class Area
{
    public int IdArea { get; set; }
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }

    public ICollection<Rol> Rol { get; set; } = new List<Rol>();
}