using AVASphere.ApplicationCore.Common.Entities.Products;
namespace AVASphere.ApplicationCore.Common.Entities.Catalogs;

public class PropertyValue
{
    public int IdPropertyValue { get; set; }
    public string? Value { get; set; }
    
    // FK
    public int IdProperty { get; set; }
    public Property Property { get; set; } = null!;
    
    // RELACIONES
    public ICollection<ProductProperties> ProductProperties { get; set; } = new List<ProductProperties>();
}