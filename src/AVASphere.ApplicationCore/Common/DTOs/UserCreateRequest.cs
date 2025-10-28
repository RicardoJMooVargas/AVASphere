using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Common.DTOs;
public class UserCreateRequest
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [StringLength(100, ErrorMessage = "El nombre de usuario no puede exceder 100 caracteres")]
    public string UserName { get; set; } = null!;

    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string? Name { get; set; }

    [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
    public string? LastName { get; set; }

    [StringLength(255, ErrorMessage = "La contraseña no puede exceder 255 caracteres")]
    public string? Password { get; set; }

    [StringLength(255, ErrorMessage = "El hash de contraseña no puede exceder 255 caracteres")]
    public string? HashPassword { get; set; }

    [StringLength(50, ErrorMessage = "El estado no puede exceder 50 caracteres")]
    public string? Status { get; set; }

    [StringLength(100, ErrorMessage = "El campo auxiliar no puede exceder 100 caracteres")]
    public string? Aux { get; set; }

    [StringLength(10, ErrorMessage = "El campo de verificación no puede exceder 10 caracteres")]
    public string? Verified { get; set; }

    [Required(ErrorMessage = "El ID del rol es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del rol debe ser mayor a 0")]
    public int IdRols { get; set; }
}