namespace AVASphere.ApplicationCore.Common.Entities;

public class Rols
{
    public int IdRols { get; set; }
    public string Name { get; set; } = null!;
    public string? NormalizedName { get; set; }

    // JSON
    public string? Permissions { get; set; }
    public string? Modules { get; set; }

    // FK
    public int IdAreas { get; set; }
    public Areas Areas { get; set; } = null!;

    public ICollection<Users> Users { get; set; } = new List<Users>();
}


public class Permissions
{
    public int Index { get; set; }
    public string? Name { get; set; } 
    public string? Normalized { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    
}
public class Modules
{
    public int Index { get; set; }
    public string? Name { get; set; } 
}