namespace AVASphere.ApplicationCore.Projects.Entities;

public class ListOfProductsToQuot
{
    public int IdListOfProductsToQuot { get; set; }
    
    // FK
    public int IdIndividualProjectQuotes { get; set; }
    public IndividualProjectQuote IndividualProjectQuotes { get; set; } = null!;
    
    /*
    public int Products { get; set; }
    public Products Products { get; set; } = null!;
    */
    
    // JSON
    public double Waste { get; set; }
}