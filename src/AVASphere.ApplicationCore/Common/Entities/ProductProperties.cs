namespace AVASphere.ApplicationCore.Common.Entities;

public class ProductProperties
{
    public int IdProductProperties { get; set; }
    public string? Value { get; set; }
    
    // Fk 
    public int IdProduct { get; set; }
    public Product Product { get; set; } = null!;
    
    public int IdCatalogValue { get; set; }
    public CatalogValue CatalogValue { get; set; } = null!;
}