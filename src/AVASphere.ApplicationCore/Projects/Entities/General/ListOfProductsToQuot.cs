// REVISED
using AVASphere.ApplicationCore.Common.Entities.Products;

namespace AVASphere.ApplicationCore.Projects.Entities.General;

public class ListOfProductsToQuot
{
    public int IdListOfProductsToQuot { get; set; }
    
    // FK
    public int IdIndividualProjectQuotes { get; set; }
    public IndividualProjectQuote IndividualProjectQuotes { get; set; } = null!;
    public int IdProduct { get; set; }
    public Product Product { get; set; } = null!;
}