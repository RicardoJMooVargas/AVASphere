    using System.ComponentModel.DataAnnotations;

    namespace AVASphere.ApplicationCore.Inventory.DTOs;

    // DTO para crear un nuevo conteo físico
    public class CreatePhysicalInventoryDto
    {
        [Required]
        public DateTime InventoryDate { get; set; } = DateTime.UtcNow;
        
        [StringLength(500, ErrorMessage = "Observations cannot exceed 500 characters")]
        public string? Observations { get; set; }
        
        [Required]
        public int IdWarehouse { get; set; }
    }

    // DTO para actualizar un conteo físico
    public class UpdatePhysicalInventoryDto
    {
        [Required]
        public int IdPhysicalInventory { get; set; }
        
        [Required]
        public DateTime InventoryDate { get; set; }
        
        [Required]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = string.Empty;
        
        [Required]
        public int CreatedBy { get; set; }
        
        [StringLength(500, ErrorMessage = "Observations cannot exceed 500 characters")]
        public string? Observations { get; set; }
        
        [Required]
        public int IdWarehouse { get; set; }
    }

    // DTO de respuesta con información del conteo físico
    public class PhysicalInventoryResponseDto
    {
        public int IdPhysicalInventory { get; set; }
        public DateTime InventoryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public string? CreatedByUserName { get; set; }
        public string? Observations { get; set; }
        public int IdWarehouse { get; set; }
        public string? WarehouseName { get; set; }
        public string? WarehouseCode { get; set; }
        public bool HasDetails { get; set; }
        public int DetailsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO para la respuesta del GET del conteo físico con productos
    public class PhysicalInventoryWithProductsDto
    {
        public int IdPhysicalInventory { get; set; }
        public DateTime InventoryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public string? CreatedByUserName { get; set; }
        public string? Observations { get; set; }
        public WarehouseInfoDto Warehouse { get; set; } = new();
        public List<ProductForInventoryDto> Products { get; set; } = new();
        public UserAreaInfoDto UserArea { get; set; } = new();
    }

    // DTO para información del warehouse
    public class WarehouseInfoDto
    {
        public int IdWarehouse { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Location { get; set; }
    }

    // DTO para productos relacionados al warehouse
    public class ProductForInventoryDto
    {
        public int IdProduct { get; set; }
        public string MainName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public double CurrentStock { get; set; }
        public LocationInfoDto? Location { get; set; }
    }

    // DTO para información de ubicación
    public class LocationInfoDto
    {
        public int IdLocationDetails { get; set; }
        public string TypeStorageSystem { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public int VerticalLevel { get; set; }
        public string AreaName { get; set; } = string.Empty;
        public string StorageStructureCode { get; set; } = string.Empty;
    }

    // DTO para información del área del usuario
    public class UserAreaInfoDto
    {
        public int IdArea { get; set; }
        public string AreaName { get; set; } = string.Empty;
        public string AreaNormalizedName { get; set; } = string.Empty;
    }
