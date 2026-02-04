﻿// REVISED
using AVASphere.ApplicationCore.Common.Entities.Products;

namespace AVASphere.ApplicationCore.Projects.Entities.General;

public class IndividualListingProperties
{
    public int IdIndividualListingProperties { get; set; }
    
    // FK
    public int IdIndividualProjectQuote { get; set; }
    public IndividualProjectQuote IndividualProjectQuote { get; set; } = null!;
    
    public int IdProductProperties { get; set; }
    public ProductProperties ProductProperties { get; set; } = null!;
}