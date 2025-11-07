namespace AVASphere.ApplicationCore.Projects.Entities.General;

public class ProjectQuote
{
    public int IdProjectQuotes { get; set; }
    public double GrandTotal { get; set; }
    public double TotalTaxes { get; set; }
    
    // FK
    public int IdProject { get; set; }
    public Project Project { get; set; } = null!;
    // Relaciones
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<IndividualProjectQuote> IndividualProjectQuotes { get; set; } = new List<IndividualProjectQuote>();
    
}