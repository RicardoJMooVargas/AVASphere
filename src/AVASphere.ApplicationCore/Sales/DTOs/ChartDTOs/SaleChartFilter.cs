namespace AVASphere.ApplicationCore.Sales.DTOs.ChartDTOs;

public class SaleByCostChartFilter
{
    // Filtros basicos de primer nivel
    public TypeFilter Type { get; set; } 
    public int? Month { get; set; } // solo funcional cuando el type es monthly
    public int? Year { get; set; } // solo funcional cuando el type es monthly
    public DateTime? SpecificDate { get; set; } // solo funcional cuando el type es daily
    public DateTime? StartDate { get; set; } // solo funcional cuando el type es personalized
    public DateTime? EndDate { get; set; } // solo funcional cuando el type es personalized
    // Filtros Avansados de 2do nivel
    public String? CustomerName { get; set; }
    public String? ProductName { get; set; }
}

public class SalesByAgentChartFilter
{
    // Filtros basicos de primer nivel
    public TypeFilter Type { get; set; } 
    public string? SalesAgent { get; set; } // usara el SalesExecutive o bien el "Agente" de la nota original
    public int? Month { get; set; } // solo funcional cuando el type es monthly
    public int? Year { get; set; } // solo funcional cuando el type es monthly
    public DateTime? SpecificDate { get; set; } // solo funcional cuando el type es daily
    public DateTime? StartDate { get; set; } // solo funcional cuando el type es personalized
    public DateTime? EndDate { get; set; } // solo funcional cuando el type es personalized
    // Filtros Avansados de 2do nivel
    public String? CustomerName { get; set; }
}

public class SalesByProductChartFilter
{
    // Filtros basicos de primer nivel
    public TypeFilter Type { get; set; } 
    public int? Month { get; set; } // solo funcional cuando el type es monthly
    public int? Year { get; set; } // solo funcional cuando el type es monthly
    public DateTime? SpecificDate { get; set; } // solo funcional cuando el type es daily
    public DateTime? StartDate { get; set; } // solo funcional cuando el type es personalized
    public DateTime? EndDate { get; set; } // solo funcional cuando el type es personalized
    // Filtros Avansados de 2do nivel
    public String? ProductName { get; set; }
}