namespace AVASphere.ApplicationCore.Common.Entities;

public class Catalog
{
    public int IdCatalog { get; set; }
    public string? Name { get; set; }
    
    // FK
    public ICollection<CatalogValue> CatalogValue { get; set; } = new List<CatalogValue>();

}