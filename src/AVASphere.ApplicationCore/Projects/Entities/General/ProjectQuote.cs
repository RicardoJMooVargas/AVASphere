// REVISED
namespace AVASphere.ApplicationCore.Projects.Entities.General;

public class ProjectQuote
{
    public int IdProjectQuotes { get; set; }
    public double GrandTotal { get; set; }
    public double TotalTaxes { get; set; }
    
    // FK - Relación 1-1 con Project
    public int IdProject { get; set; }
    public Project Project { get; set; } = null!;
    
    // Relación 1-N con IndividualProjectQuotes
    public ICollection<IndividualProjectQuote> IndividualProjectQuotes { get; set; } = new List<IndividualProjectQuote>();
}