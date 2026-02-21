    using System.ComponentModel.DataAnnotations;

    namespace AVASphere.ApplicationCore.Inventory.DTOs;

    public class LocationDetailsRequestDto
    {
        [Required(ErrorMessage = "La sección es requerida")]
        [StringLength(100, ErrorMessage = "La sección no puede exceder 100 caracteres")]
        public string Section { get; set; } = null!;
        
        [Required(ErrorMessage = "El nivel vertical es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El nivel vertical debe ser mayor a 0")]
        public int VerticalLevel { get; set; }
        
        public int? IdArea { get; set; } // Opcional, si no se proporciona se toma del usuario
        
        [Required(ErrorMessage = "El ID de la estructura de almacenamiento es requerido")]
        public int IdStorageStructure { get; set; }
    }

    public class LocationDetailsUpdateDto
    {
        [Required(ErrorMessage = "La sección es requerida")]
        [StringLength(100, ErrorMessage = "La sección no puede exceder 100 caracteres")]
        public string Section { get; set; } = null!;
        
        [Required(ErrorMessage = "El nivel vertical es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El nivel vertical debe ser mayor a 0")]
        public int VerticalLevel { get; set; }
        
        [Required(ErrorMessage = "El ID del área es requerido")]
        public int IdArea { get; set; }
        
        [Required(ErrorMessage = "El ID de la estructura de almacenamiento es requerido")]
        public int IdStorageStructure { get; set; }
    }

    public class LocationDetailsResponseDto
    {
        public int IdLocationDetails { get; set; }
        public string Section { get; set; } = null!;
        public int VerticalLevel { get; set; }
        public int IdArea { get; set; }
        public string? AreaName { get; set; }
        public string? AreaNormalizedName { get; set; }
        public int IdStorageStructure { get; set; }
        public string? CodeRack { get; set; }
        public string? TypeStorageSystem { get; set; } // Viene de StorageStructure
    }
