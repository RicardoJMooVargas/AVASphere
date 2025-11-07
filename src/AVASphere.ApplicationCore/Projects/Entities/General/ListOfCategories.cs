using AVASphere.ApplicationCore.Projects.Entities.Catalogs;
using AVASphere.ApplicationCore.Projects.Entities.jsons;

namespace AVASphere.ApplicationCore.Projects.Entities.General;

public class ListOfCategories
{
    public int IdListOfCategories { get; set; }
    
    // FK
    public int IdProject { get; set; }
    public Project Project { get; set; } = null!;
    
    public int IdProjectCategory { get; set; }
    public ProjectCategory ProjectCategory { get; set; } = null!;
    
    // JSON
    public SolutionsJson SolutionsJson { get; set; } = null!;
    public string? Properties { get; set; }
    public string? Products { get; set; }
}

