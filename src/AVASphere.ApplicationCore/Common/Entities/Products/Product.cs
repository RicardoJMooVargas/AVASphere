using AVASphere.ApplicationCore.Common.Entities.General;

namespace AVASphere.ApplicationCore.Common.Entities.Products;

public class Product
{
    public int IdProduct { get; set; }
    public string? MainName { get; set; }
    public string? SupplierName { get; set; }
    public int Unit { get; set; }
    public string? Description { get; set; }
    public double Quantity { get; set; }
    public double Taxes { get; set; }
    
    // FK
    public int IdSupplier { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public ICollection<ProductProperties> ProductProperties { get; set; } = new List<ProductProperties>();
    
    //JSON
    public ICollection<CodeJson> CodeJson { get; set; } = new List<CodeJson>();
    public ICollection<CostsJson> CostsJson { get; set; } = new List<CostsJson>();
    public ICollection<CategoriesJson> CategoriesJsons { get; set; } = new List<CategoriesJson>();
    public ICollection<SolutionsJson> SolutionsJsons { get; set; } = new List<SolutionsJson>();
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