using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Projects.Entities.General;

namespace AVASphere.ApplicationCore.Projects.Entities.Catalogs;

public class ProjectCategory
{
    public int IdProjectCategory { get; set; }
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }
    
    // FK 
   // public int IdConfigSys { get; set; }
   // public ConfigSys ConfigSys { get; set; } = null!;
    // Relaciones 
   // public ICollection<IndividualProjectQuote> IndividualProjectQuotes { get; set; } = new List<IndividualProjectQuote>();
   // public ICollection<ListOfCategories> ListOfCategories { get; set; } = new List<ListOfCategories>();
   // public ICollection<ListOfProductsByCategory> ListOfProductsByCategory { get; set; } = new List<ListOfProductsByCategory>();
   // public ICollection<TechnicalDesign> TechnicalDesigns { get; set; } = new List<TechnicalDesign>();
}