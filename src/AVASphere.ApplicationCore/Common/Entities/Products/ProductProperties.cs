using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.ApplicationCore.Projects.Entities.General;

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
    
    //Relaciones
    public ICollection<IndividualListingProperties> IndividualListingProperties { get; set; } = new List<IndividualListingProperties>();

}