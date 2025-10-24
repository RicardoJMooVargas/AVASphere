namespace AVASphere.ApplicationCore.Common.Entities;


public class Areas
{
    public int IdAreas { get; set; }
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }

    public ICollection<Rols> Rols { get; set; } = new List<Rols>();
}