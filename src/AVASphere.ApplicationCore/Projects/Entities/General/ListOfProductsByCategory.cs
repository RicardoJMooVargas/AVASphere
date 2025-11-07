using AVASphere.ApplicationCore.Projects.Entities.Catalogs;
using AVASphere.ApplicationCore.Projects.Entities.jsons;

namespace AVASphere.ApplicationCore.Projects.Entities;

public class ListOfProductsByCategory
{
    public int IdListOfProductsByCategory { get; set; }
    
    // FK
    public int IdProjectCategory { get; set; }
    public ProjectCategory ProjectCategory { get; set; } = null!;
    
    // JSON
    public SolutionsJson SolutionsJson { get; set; } = null!;
}

