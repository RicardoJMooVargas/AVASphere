namespace AVASphere.ApplicationCore.Common.Entities;

public class CatalogValue
{
    public int IdCatalogValue { get; set; }
    public string? Value { get; set; }
    
    // FK
    public int IdCatalog { get; set; }
    public Catalog Catalog { get; set; } = null!;
    
    public ICollection<ProductProperties> ProductProperties { get; set; } = new List<ProductProperties>();
}