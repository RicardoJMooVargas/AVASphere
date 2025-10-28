using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Common.DTOs;


public class LoginRequest
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es requerida")]
    public string Password { get; set; } = null!;
}
public class LoginResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public UserResponse? User { get; set; }
    public string? Message { get; set; }
    
    // Métodos helper para facilitar el uso
    public static LoginResponse SuccessResponse(string token, UserResponse user)
    {
        return new LoginResponse
        {
            Success = true,
            Token = token,
            User = user,
            Message = "Autenticación exitosa"
        };
    }
    
    public static LoginResponse FailureResponse(string message)
    {
        return new LoginResponse
        {
            Success = false,
            Message = message
        };
    }
}