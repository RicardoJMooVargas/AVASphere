namespace AVASphere.ApplicationCore.Common.Entities;
public class User
{
    public int IdUsers { get; set; }
    public string UserName { get; set; } = null!;
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string? Password { get; set; }
    public string? HashPassword { get; set; }
    public string? Status { get; set; }
    public string? Aux { get; set; }
    public string? CreateAt { get; set; }
    public string? Verified { get; set; }

    // FK
    public int IdRol { get; set; }
    public Rol Rol { get; set; } = null!;
}