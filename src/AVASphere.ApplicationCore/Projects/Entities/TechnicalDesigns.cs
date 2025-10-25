namespace AVASphere.ApplicationCore.Projects.Entities;

public class TechnicalDesigns
{
    public int IdTechnicalDesign { get; set; }
    public string? SavedDesign { get; set; }
    
    // FK
    public int IdProjectCategory { get; set; }
    public ProjectCategory ProjectCategory { get; set; } = null!;
}