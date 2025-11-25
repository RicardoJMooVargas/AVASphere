// DEPRECATED: This class is no longer in use and will be removed in future versions.
using AVASphere.ApplicationCore.Projects.Entities.Catalogs;
using AVASphere.ApplicationCore.Projects.Entities.jsons;

namespace AVASphere.ApplicationCore.Projects.Entities.General;

public class ListOfProductsByCategory
{
    public int IdListOfProductsByCategory { get; set; }
    
    // FK
    public int IdProjectCategory { get; set; }
    public ProjectCategory ProjectCategory { get; set; } = null!;
    
    // JSON
    public SolutionsJson SolutionsJson { get; set; } = null!;
}

