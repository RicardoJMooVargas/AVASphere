using AVASphere.ApplicationCore.Projects.Entities.Catalogs;

namespace AVASphere.ApplicationCore.Projects.Entities;

public class TechnicalDesign
{
    public int IdTechnicalDesign { get; set; }
    public string? SavedDesign { get; set; }
    
    // FK
    public int IdProjectCategory { get; set; }
    public ProjectCategory ProjectCategory { get; set; } = null!;
}