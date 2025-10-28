using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Common.DTOs;

public class UserUpdateRequest
{
    [Required(ErrorMessage = "El ID de usuario es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario debe ser mayor a 0")]
    public int IdUsers { get; set; }

    [StringLength(100, ErrorMessage = "El nombre de usuario no puede exceder 100 caracteres")]
    public string? UserName { get; set; }

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

    [Range(1, int.MaxValue, ErrorMessage = "El ID del rol debe ser mayor a 0")]
    public int? IdRols { get; set; }
}
