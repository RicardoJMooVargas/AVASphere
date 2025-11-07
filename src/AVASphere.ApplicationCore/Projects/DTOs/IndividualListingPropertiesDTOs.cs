namespace AVASphere.ApplicationCore.Projects.DTOs;

public class IndividualListingPropertiesRequestDto
{
    public int IdIndividualProjectQuote { get; set; }
    public int IdProductProperties { get; set; }
}

public class IndividualListingPropertiesResponsetDto
{
    public int IdIndividualListingProperties { get; set; }
    public int IdIndividualProjectQuote { get; set; }
    public int IdProductProperties { get; set; }
}