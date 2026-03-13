using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.System.DTOs;

/// <summary>
/// DTO para crear usuario administrador
/// </summary>
public class AdminUserRequestDto
{
    /// <summary>
    /// Nombre de usuario del administrador
    /// </summary>
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del administrador
    /// </summary>
    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string Password { get; set; } = string.Empty;
}


