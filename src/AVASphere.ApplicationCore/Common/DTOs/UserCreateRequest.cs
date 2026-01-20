using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Common.DTOs;

public class UserCreateRequest
{

    public string UserName { get; set; } = null!;
    public string? Name { get; set; }

    public string? LastName { get; set; }

    public string Password { get; set; } = null!;

    public string? Aux { get; set; }

    public bool? Verified { get; set; } = false;

    public int IdRols { get; set; }

    public int IdConfigSys { get; set; }
}