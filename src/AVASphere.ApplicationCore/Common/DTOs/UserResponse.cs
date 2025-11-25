using AVASphere.ApplicationCore.Common.Entities.General;

namespace AVASphere.ApplicationCore.Common.DTOs;

public class UserResponse
{
    public int IdUsers { get; set; }
    public string UserName { get; set; } = null!;
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string? Status { get; set; }
    public string? Aux { get; set; }
    public string? CreateAt { get; set; }
    public string? Verified { get; set; }
    public int IdRols { get; set; }
    public string? RolName { get; set; }
    
    // ✅ NUEVAS PROPIEDADES PARA CONFIG SYS
    public int IdConfigSys { get; set; }
    public string? ConfigSysName { get; set; }
    public string? CompanyName { get; set; }
    public string? BranchName { get; set; }
    public string? LogoUrl { get; set; }
    // Configuraciones y permisos
    public List<Module> Modules { get; set; } = new List<Module>();
    public List<Permission> Permissions { get; set; } = new List<Permission>();
}

public class AuthUserResponse
{
    public int IdUsers { get; set; }
    public string UserName { get; set; } = null!;
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string? Status { get; set; }
    public string? Aux { get; set; }
    public string? Hash { get; set; }
    public string? CreateAt { get; set; }
    public string? Verified { get; set; }
    public int IdRols { get; set; }
    public string? RolName { get; set; }
    
    // ✅ NUEVAS PROPIEDADES PARA CONFIG SYS
    public int IdConfigSys { get; set; }
    public string? ConfigSysName { get; set; }
    public string? CompanyName { get; set; }
    public string? BranchName { get; set; }
    public string? LogoUrl { get; set; }
}