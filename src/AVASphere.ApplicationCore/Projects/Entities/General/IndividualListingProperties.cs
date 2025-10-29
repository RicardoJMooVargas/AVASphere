namespace AVASphere.ApplicationCore.Projects.Entities.General;

public class IndividualListingProperties
{
    public int IdIndividualListingProperties { get; set; }
    
    // FK
    public int IdIndividualProjectQuotes { get; set; }
    public IndividualProjectQuote IndividualProjectQuotes { get; set; } = null!;
}