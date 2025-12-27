namespace AVASphere.ApplicationCore.Common.Entities.General;
public class User
{
    public int IdUser { get; set; }
    public string UserName { get; set; } = null!; // no se puede repetir
    public string Name { get; set; }
    public string LastName { get; set; }
    public string HashPassword { get; set; }
    public string Status { get; set; }
    public string? Aux { get; set; }
    public DateOnly? CreateAt { get; set; }
    public bool? Verified { get; set; } = false;

    // 🔹 Relación con rol (1-N)
    public int IdRol { get; set; }
    public Rol Rol { get; set; } = null!;
    // 🔹 Relación con rol (1-N)
    public int IdConfigSys { get; set; }
    public ConfigSys ConfigSys { get; set; } = null!;
    
    
}