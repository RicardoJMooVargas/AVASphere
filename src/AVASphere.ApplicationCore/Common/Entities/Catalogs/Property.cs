namespace AVASphere.ApplicationCore.Common.Entities.Catalogs;

public class Property
{
    public int IdProperty { get; set; }
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }
    
    
    // RELACIONES
    public ICollection<PropertyValue> CatalogValue { get; set; } = new List<PropertyValue>(); //Cambiar el nombre de la variable por PropertyValue
}