using System.ComponentModel.DataAnnotations.Schema;
using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using AVASphere.ApplicationCore.Projects.Entities.General;
using AVASphere.ApplicationCore.Projects.Entities.jsons;
using SolutionsJsonProject = AVASphere.ApplicationCore.Projects.Entities.jsons.SolutionsJson;

namespace AVASphere.ApplicationCore.Common.Entities.Products;

public class Product
{
    public int IdProduct { get; set; }
    public string? MainName { get; set; }
    public string? Unit { get; set; }
    public string? Description { get; set; }
    public double Quantity { get; set; }
    public double Taxes { get; set; }

    // 🔹 NUEVO CAMPO JSON PARA IMÁGENES
    public ICollection<ProductImageJson> ImageUrls { get; set; } = new List<ProductImageJson>();

    // FK
    public int IdSupplier { get; set; }
    public Supplier Supplier { get; set; } = null!;

    // RELACIONES
    public ICollection<ProductProperties> ProductProperties { get; set; } = new List<ProductProperties>();
    public ICollection<ListOfProductsToQuot> ProductImages { get; set; } = new List<ListOfProductsToQuot>();
    // Las siguientes propiedades de navegación fueron eliminadas para evitar campos duplicados:
    // - Inventories, PhysicalInventoryDetails, StockMovements, WarehouseTransferDetails
    // Las relaciones se configuran usando WithMany() sin especificar la propiedad inversa

    //JSON
    public ICollection<CodeJson> CodeJson { get; set; } = new List<CodeJson>();
    public ICollection<CostsJson> CostsJson { get; set; } = new List<CostsJson>();
    public ICollection<CategoriesJson> CategoriesJsons { get; set; } = new List<CategoriesJson>();

    [NotMapped]
    public ICollection<SolutionsJsonProject> SolutionsJsons { get; set; } = new List<SolutionsJsonProject>();

    public AuxDataJson? AuxDataJson { get; set; } = new();
    
    // GETTERS
    public string FirstCode => CodeJson?.FirstOrDefault()?.Code ?? string.Empty;
}

public class ProductImageJson
{
    public int Index { get; set; }
    public string? Url { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }   // image/png, image/jpeg, etc.
    public bool IsMain { get; set; }           // Imagen principal
}

public class CodeJson
{
    public int Index { get; set; }
    public string? Type { get; set; }
    public string? Code { get; set; }
}

public class CostsJson
{
    public int Index { get; set; }
    public double Amount { get; set; }
    public string? Type { get; set; }
}

public class CategoriesJson
{
    public int Index { get; set; }
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }
}

public class AuxDataJson
{
}
