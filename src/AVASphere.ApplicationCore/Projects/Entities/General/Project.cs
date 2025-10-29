using AVASphere.ApplicationCore.Common.Entities.General;

namespace AVASphere.ApplicationCore.Projects.Entities.General;
using AVASphere.ApplicationCore.Common.Entities;

public class Project
{
    public int IdProject { get; set; }
    public string? WrittenAddress { get; set; } 
    public string? ExactAddress { get; set; }
    
    // JSON
    public AppointmentJson? AppointmentJson { get; set; }
    
    //FK
    public int IdProjectQuotes { get; set; }
    public ProjectQuote ProjectQuote { get; set; } = null!;
    
    public int IdVisits { get; set; }
    public Visit Visit { get; set; } = null!;
    
    public ICollection<ListOfCategories> ListOfCategories { get; set; } = new List<ListOfCategories>();
    public ICollection<Customer> Customer { get; set; } = new List<Customer>();


}

public class AppointmentJson
{
    public int Index { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime Datetime { get; set; }
    public string? Notes { get; set; }
    public string? Direction { get; set; }
    public string? Reference { get; set; }
}

