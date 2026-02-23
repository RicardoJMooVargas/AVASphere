using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Projects.Entities.jsons;

namespace AVASphere.ApplicationCore.Common.DTOs.ProductDTOs;

public class ProductResponseDto
{
    public int IdProduct { get; set; }
    public string MainName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public double Taxes { get; set; }
    public int IdSupplier { get; set; }
    public List<ProductImageJson> ImageUrls { get; set; } = new List<ProductImageJson>();
    public List<CodeJson> CodeJson { get; set; } = new();
    public List<CostsJson> CostsJson { get; set; } = new();
    public List<CategoriesJson> CategoriesJsons { get; set; } = new();
    public List<SolutionsJson> SolutionsJsons { get; set; } = new();
    public List<ProductPropertyDto> ProductProperties { get; set; } = new();
}