using System.ComponentModel.DataAnnotations;
using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Projects.Entities.jsons;

namespace AVASphere.ApplicationCore.Common.DTOs.ProductDTOs;

public class UpdateProductDto
{
    public string? MainName { get; set; }
    public string? Unit { get; set; }
    public string? Description { get; set; }
    public double? Quantity { get; set; }
    public double? Taxes { get; set; }
    public int? IdSupplier { get; set; }

    public List<CodeJson>? CodeJson { get; set; }
    public List<CostsJson>? CostsJson { get; set; }
    public List<CategoriesJson>? CategoriesJsons { get; set; }
    public List<SolutionsJson>? SolutionsJsons { get; set; }
}