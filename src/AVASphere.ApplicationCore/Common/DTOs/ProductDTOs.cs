using System.ComponentModel.DataAnnotations;
using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Projects.Entities.jsons;

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

    public List<CodeJson>? CodeJson { get; set; }
    public List<CostsJson>? CostsJson { get; set; }
    public List<CategoriesJson>? CategoriesJsons { get; set; }
    public List<SolutionsJson>? SolutionsJsons { get; set; }
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
