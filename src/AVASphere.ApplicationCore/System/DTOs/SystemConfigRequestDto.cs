using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.System.DTOs;

/// <summary>
/// DTO para configuración del sistema
/// </summary>
public class SystemConfigRequestDto
{
    /// <summary>
    /// Nombre de la compañía
    /// </summary>
    [Required(ErrorMessage = "El nombre de la compañía es requerido")]
    [StringLength(200, ErrorMessage = "El nombre de la compañía no puede exceder 200 caracteres")]
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de la sucursal
    /// </summary>
    [StringLength(200, ErrorMessage = "El nombre de la sucursal no puede exceder 200 caracteres")]
    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// URL del logo de la compañía
    /// </summary>
    [StringLength(500, ErrorMessage = "La URL del logo no puede exceder 500 caracteres")]
    public string LogoUrl { get; set; } = string.Empty;

    /// <summary>
    /// Colores personalizados del sistema
    /// </summary>
    public List<ColorJsonDto>? Colors { get; set; }

    /// <summary>
    /// Módulos que no se utilizarán en el sistema
    /// </summary>
    public List<int>? NotUseModules { get; set; }
}

/// <summary>
/// DTO para configuración de colores
/// </summary>
public class ColorJsonDto
{
    /// <summary>
    /// Índice del color
    /// </summary>
    [Required(ErrorMessage = "El índice del color es requerido")]
    public int Index { get; set; }

    /// <summary>
    /// Nombre del color
    /// </summary>
    [Required(ErrorMessage = "El nombre del color es requerido")]
    [StringLength(100, ErrorMessage = "El nombre del color no puede exceder 100 caracteres")]
    public string NameColor { get; set; } = string.Empty;

    /// <summary>
    /// Código hexadecimal del color
    /// </summary>
    [Required(ErrorMessage = "El código del color es requerido")]
    [StringLength(20, ErrorMessage = "El código del color no puede exceder 20 caracteres")]
    public string ColorCode { get; set; } = string.Empty;

    /// <summary>
    /// Valor RGB del color
    /// </summary>
    [StringLength(50, ErrorMessage = "El RGB del color no puede exceder 50 caracteres")]
    public string ColorRgb { get; set; } = string.Empty;
}
