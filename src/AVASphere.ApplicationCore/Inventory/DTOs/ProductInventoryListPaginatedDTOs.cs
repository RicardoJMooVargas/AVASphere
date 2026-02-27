using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Inventory.DTOs;

/// <summary>
/// DTO para filtros de búsqueda en la lista de productos de inventario físico
/// </summary>
public class ProductInventoryListFiltersDto
{
    /// <summary>
    /// Filtro por descripción o nombre del producto
    /// </summary>
    public string? SearchText { get; set; }
    
    /// <summary>
    /// Filtro por ID del proveedor
    /// </summary>
    public int? IdSupplier { get; set; }
    
    /// <summary>
    /// Filtro por familia (propiedad del producto)
    /// </summary>
    public string? Familia { get; set; }
    
    /// <summary>
    /// Filtro por clase (propiedad del producto)
    /// </summary>
    public string? Clase { get; set; }
    
    /// <summary>
    /// Filtro por línea (propiedad del producto)
    /// </summary>
    public string? Linea { get; set; }
}

/// <summary>
/// DTO para parámetros de paginación
/// </summary>
public class ProductInventoryListPaginationDto
{
    /// <summary>
    /// Número de página (empezando en 1)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "El número de página debe ser mayor a 0")]
    public int PageNumber { get; set; } = 1;
    
    /// <summary>
    /// Número de elementos por página
    /// </summary>
    [Range(1, 1000, ErrorMessage = "El tamaño de página debe estar entre 1 y 1000")]
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// DTO para información de catálogos/listas para filtros
/// </summary>
public class ProductInventoryCatalogsDto
{
    /// <summary>
    /// Lista de proveedores disponibles en los productos del inventario físico
    /// </summary>
    public List<SupplierFilterDto> Suppliers { get; set; } = new();
    
    /// <summary>
    /// Lista de valores únicos de familia encontrados en los productos
    /// </summary>
    public List<string> Familias { get; set; } = new();
    
    /// <summary>
    /// Lista de valores únicos de clase encontrados en los productos
    /// </summary>
    public List<string> Clases { get; set; } = new();
    
    /// <summary>
    /// Lista de valores únicos de línea encontrados en los productos
    /// </summary>
    public List<string> Lineas { get; set; } = new();
}

/// <summary>
/// DTO simplificado para proveedor en filtros
/// </summary>
public class SupplierFilterDto
{
    public int IdSupplier { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
}

/// <summary>
/// DTO de respuesta paginada para lista de productos de inventario físico
/// </summary>
public class ProductInventoryListPaginatedResponseDto
{
    /// <summary>
    /// Lista paginada de productos
    /// </summary>
    public List<ProductInventoryListDto> Products { get; set; } = new();
    
    /// <summary>
    /// Información de paginación
    /// </summary>
    public PaginationMetadataDto Pagination { get; set; } = new();
    
    /// <summary>
    /// Catálogos/listas para filtros en el frontend
    /// </summary>
    public ProductInventoryCatalogsDto Catalogs { get; set; } = new();
    
    /// <summary>
    /// Información adicional del inventario
    /// </summary>
    public ProductInventoryInfoDto InventoryInfo { get; set; } = new();
}

/// <summary>
/// DTO para metadatos de paginación
/// </summary>
public class PaginationMetadataDto
{
    /// <summary>
    /// Página actual
    /// </summary>
    public int CurrentPage { get; set; }
    
    /// <summary>
    /// Tamaño de página
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Total de registros
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Total de páginas
    /// </summary>
    public int TotalPages { get; set; }
    
    /// <summary>
    /// Indica si hay página anterior
    /// </summary>
    public bool HasPrevious { get; set; }
    
    /// <summary>
    /// Indica si hay página siguiente
    /// </summary>
    public bool HasNext { get; set; }
}

/// <summary>
/// DTO para información adicional del inventario
/// </summary>
public class ProductInventoryInfoDto
{
    public bool HasInventoryRecords { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string UserAreaName { get; set; } = string.Empty;
    public int TotalProductsInInventory { get; set; }
    public int FilteredProductsCount { get; set; }
}
