// REVISED
using AVASphere.ApplicationCore.Projects.Entities.Catalogs;
using AVASphere.ApplicationCore.Projects.Entities.jsons;

namespace AVASphere.ApplicationCore.Projects.Entities.General;

public class TechnicalDesign
{
    public int IdTechnicalDesign { get; set; }
    public string? SavedDesign { get; set; }
    public string? imageUrl { get; set; }
    // JSON
    public ICollection<SolutionsJson> SolutionsJsons { get; set; } = new List<SolutionsJson>();
    
    // FK
    public int IdProjectCategory { get; set; }
    public ProjectCategory ProjectCategory { get; set; } = null!;
}