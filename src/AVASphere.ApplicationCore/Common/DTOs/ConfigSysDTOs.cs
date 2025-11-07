using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Common.DTOs
{
    // DTO para crear o actualizar ConfigSys
    public class ConfigSysRequestDto
    {
        [Required(ErrorMessage = "El nombre de la compañía es requerido")]
        [StringLength(200, ErrorMessage = "El nombre de la compañía no puede exceder 200 caracteres")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "El nombre de la sucursal no puede exceder 200 caracteres")]
        public string BranchName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La URL del logo no puede exceder 500 caracteres")]
        public string LogoUrl { get; set; } = string.Empty;

        public List<ColorRequestDto> Colors { get; set; } = new List<ColorRequestDto>();
        public List<NotUseModuleRequestDto> NotUseModules { get; set; } = new List<NotUseModuleRequestDto>();
    }

    // DTO para respuesta de ConfigSys
    public class ConfigSysResponseDto
    {
        public int IdConfigSys { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public List<ColorResponseDto> Colors { get; set; } = new List<ColorResponseDto>();
        public List<NotUseModuleResponseDto> NotUseModules { get; set; } = new List<NotUseModuleResponseDto>();
        public DateTime CreatedAt { get; set; }
    }

    // DTOs para Colors
    public class ColorRequestDto
    {
        [Required(ErrorMessage = "El índice del color es requerido")]
        public int Index { get; set; }

        [Required(ErrorMessage = "El nombre del color es requerido")]
        [StringLength(100, ErrorMessage = "El nombre del color no puede exceder 100 caracteres")]
        public string NameColor { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código del color es requerido")]
        [StringLength(20, ErrorMessage = "El código del color no puede exceder 20 caracteres")]
        public string ColorCode { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "El RGB del color no puede exceder 50 caracteres")]
        public string ColorRgb { get; set; } = string.Empty;
    }

    public class ColorResponseDto
    {
        public int Index { get; set; }
        public string NameColor { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;
        public string ColorRgb { get; set; } = string.Empty;
    }

    // DTOs para NotUseModules
    public class NotUseModuleRequestDto
    {
        [Required(ErrorMessage = "El índice del módulo es requerido")]
        public int Index { get; set; }

        [Required(ErrorMessage = "El nombre del módulo es requerido")]
        [StringLength(100, ErrorMessage = "El nombre del módulo no puede exceder 100 caracteres")]
        public string NameModule { get; set; } = string.Empty;
    }

    public class NotUseModuleResponseDto
    {
        public int Index { get; set; }
        public string NameModule { get; set; } = string.Empty;
    }
}