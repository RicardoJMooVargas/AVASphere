using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Common.DTOs;

public class UserSearchRequest
{
    [Required(ErrorMessage = "El ID de usuario es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario debe ser mayor a 0")]
    public int IdUsers { get; set; }
}