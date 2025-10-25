namespace AVASphere.ApplicationCore.Projects.Entities;

public class ListOfCategories
{
    public int IdListOfCategories { get; set; }
    
    // FK
    public int IdProject { get; set; }
    public Projects Projects { get; set; } = null!;
    
    public int IdProjectCategory { get; set; }
    public ProjectCategory ProjectCategory { get; set; } = null!;
    
    // JSON
    public SolutionsJson SolutionsJson { get; set; } = null!;
    public string? Properties { get; set; }
    public string? Products { get; set; }
}

