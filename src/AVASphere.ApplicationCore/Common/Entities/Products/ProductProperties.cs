using AVASphere.ApplicationCore.Common.Entities.Catalogs;

namespace AVASphere.ApplicationCore.Common.Entities.Products;

public class ProductProperties
{
    public int IdProductProperties { get; set; }
    public string? CustomValue { get; set; }
    
    // Fk 
    public int IdProduct { get; set; }
    public Product Product { get; set; } = null!;
    
    
    public int IdPropertyValue { get; set; }
    public PropertyValue PropertyValue { get; set; } = null!;
}