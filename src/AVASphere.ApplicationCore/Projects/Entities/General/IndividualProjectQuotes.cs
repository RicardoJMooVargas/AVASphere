using AVASphere.ApplicationCore.Projects.Entities.Catalogs;

namespace AVASphere.ApplicationCore.Projects.Entities.General;

public class IndividualProjectQuote
{
    public int IdIndividualProjectQuote { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public double Quantity { get; set; }
    public double UnitPrice { get; set; }
    public double Amount { get; set; }
    public double Total { get; set; }
    public double TotalWaste { get; set; }
  
    // FK
    public int IdProjectQuotes { get; set; }
    public ProjectQuote ProjectQuote { get; set; } = null!;
  
    public int IdProjectCategory { get; set; }
    public ProjectCategory ProjectCategory { get; set; } = null!;
  
    public ICollection<IndividualListingProperties> IndividualListingProperties { get; set; } = new List<IndividualListingProperties>();
    public ICollection<ListOfProductsToQuot> ListOfProductsToQuot { get; set; } = new List<ListOfProductsToQuot>();

}