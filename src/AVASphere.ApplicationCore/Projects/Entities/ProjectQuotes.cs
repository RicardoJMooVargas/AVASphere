namespace AVASphere.ApplicationCore.Projects.Entities;

public class ProjectQuotes
{
    public int IdProjectQuotes { get; set; }
    public double GrandTotal { get; set; }
    public double TotalTaxes { get; set; }
    
    //FK
    public int IdProject { get; set; }
    public Projects Project { get; set; } = null!;
    
    public ICollection<Projects> Projects { get; set; } = new List<Projects>();
    public ICollection<IndividualProjectQuote> IndividualProjectQuotes { get; set; } = new List<IndividualProjectQuote>();
}