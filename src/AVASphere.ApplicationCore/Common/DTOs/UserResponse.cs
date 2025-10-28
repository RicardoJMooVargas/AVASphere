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
}