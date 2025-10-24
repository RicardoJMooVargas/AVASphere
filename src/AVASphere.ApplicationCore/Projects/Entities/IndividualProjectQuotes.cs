namespace AVASphere.ApplicationCore.Projects.Entities;

public class IndividualProjectQuote
{
  public int IdIndividualProjectQuote { get; set; }
  public string? Description { get; set; }
  public string? Category { get; set; }
  public double Quantity { get; set; }
  public double UnitPrice { get; set; }
  public double Amount { get; set; }
  public double Total { get; set; }
  public double TotalWaste { get; set; }
}