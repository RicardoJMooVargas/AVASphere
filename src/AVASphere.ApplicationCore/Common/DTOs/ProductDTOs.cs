using System.ComponentModel.DataAnnotations;
using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Projects.Entities.jsons;
using SolutionsJsonProject = AVASphere.ApplicationCore.Projects.Entities.jsons.SolutionsJson;

namespace AVASphere.ApplicationCore.Common.DTOs.ProductDTOs;

public class CreateProductDto
{
    public string MainName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public double Taxes { get; set; }
    public int IdSupplier { get; set; }
    public List<ProductImageJson>? ImageUrls { get; set; } = new List<ProductImageJson>();

    public List<CodeJson>? CodeJson { get; set; }
    public List<CostsJson>? CostsJson { get; set; }
    public List<CategoriesJson>? CategoriesJsons { get; set; }
    public List<SolutionsJsonProject>? SolutionsJsons { get; set; }
}


public class ImportProductDto
{
    public string MainName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public double Taxes { get; set; }
}

public class ImportProductResultDto
{
    public int TotalRows { get; set; }
    public int SuccessfulImports { get; set; }
    public int FailedImports { get; set; }
    public List<string> Errors { get; set; } = new();
}
public class ProductFilterDto
{
    public int? Id { get; set; }
    public string? MainName { get; set; }
    public int? IdSupplier { get; set; }
    public string? SupplierName { get; set; }

    /// <summary>
    /// Filtro por ID de Property (ej: 1=Familia, 2=Clase, 3=Línea)
    /// </summary>
    public int? IdProperty { get; set; }

    /// <summary>
    /// Filtro por nombre de Property (ej: "Familia", "Clase", "Línea")
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    /// Filtro por ID de PropertyValue (ej: 1=ACRILICOS)
    /// </summary>
    public int? IdPropertyValue { get; set; }

    /// <summary>
    /// Filtro por valor de PropertyValue (ej: "ACRILICOS", "CORTE")
    /// </summary>
    public string? PropertyValue { get; set; }

    /// <summary>
    /// Filtros dinámicos por propiedades. 
    /// Key: Nombre de la propiedad (ej: "Familia", "Clase", "Línea")
    /// Value: Valor a buscar
    /// </summary>
    public Dictionary<string, string>? Properties { get; set; }
}
public class ProductPropertyDto
{
    public int IdProductProperties { get; set; }
    public string? CustomValue { get; set; }
    public int IdProduct { get; set; }
    public int IdPropertyValue { get; set; }

    // Información de la PropertyValue
    public string PropertyValueName { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
}

public class PaginationDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// DTO para respuesta paginada de productos
/// </summary>
public class PaginatedProductResponseDto
{
    /// <summary>
    /// Lista de productos en la página actual
    /// </summary>
    public IEnumerable<ProductResponseDto> Items { get; set; } = new List<ProductResponseDto>();

    /// <summary>
    /// Número de página actual (base 1)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Tamaño de página (cantidad de registros por página)
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total de registros en la base de datos (considerando filtros)
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total de páginas disponibles
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indica si hay página anterior
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// Indica si hay página siguiente
    /// </summary>
    public bool HasNextPage { get; set; }
}
