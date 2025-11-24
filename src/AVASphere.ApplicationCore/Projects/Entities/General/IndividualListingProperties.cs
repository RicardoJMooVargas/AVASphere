// REVISED
using AVASphere.ApplicationCore.Common.Entities.Catalogs;

namespace AVASphere.ApplicationCore.Projects.Entities.General;

public class IndividualListingProperties
{
    public int IdIndividualListingProperties { get; set; }
    
    // FK
    public int IdIndividualProjectQuote { get; set; }
    public IndividualProjectQuote IndividualProjectQuote { get; set; } = null!;
    
    public int IdPropertyValue { get; set; }
    public PropertyValue ProductValue { get; set; } = null!;
}