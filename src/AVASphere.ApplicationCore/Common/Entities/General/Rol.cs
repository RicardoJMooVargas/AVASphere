namespace AVASphere.ApplicationCore.Common.Entities.General;

public class Rol
{
    public int IdRol { get; set; }
    public string Name { get; set; } = null!;
    public string? NormalizedName { get; set; }

    // JSON
    public List<Permission>? Permissions { get; set; }
    public List<Module>? Modules { get; set; }

    // FK
    public int IdArea { get; set; }
    public Area Area { get; set; } = null!;

    public ICollection<User> User { get; set; } = new List<User>();
}


public class Permission
{
    public int Index { get; set; }
    public string? Name { get; set; } 
    public string? Normalized { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    
}
public class Module
{
    public int Index { get; set; }
    public string? Name { get; set; } 
}