namespace AVASphere.ApplicationCore.Projects.Entities;

public class ProjectCategory
{
    public int IdProjectCategory { get; set; }
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }
    
    // FK
    public ICollection<IndividualProjectQuote> IndividualProjectQuotes { get; set; } = new List<IndividualProjectQuote>();
    public ICollection<ListOfCategories> ListOfCategories { get; set; } = new List<ListOfCategories>();
    public ICollection<ListOfProductsByCategory> ListOfProductsByCategory { get; set; } = new List<ListOfProductsByCategory>();
    public ICollection<TechnicalDesigns> TechnicalDesignsCollection { get; set; } = new List<TechnicalDesigns>();
}