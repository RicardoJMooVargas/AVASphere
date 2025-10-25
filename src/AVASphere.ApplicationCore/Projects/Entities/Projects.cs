namespace AVASphere.ApplicationCore.Projects.Entities;

public class Projects
{
    public int IdProject { get; set; }
    public string? WrittenAddress { get; set; } 
    public string? ExactAddress { get; set; }
    
    // JSON
    public string? Appointment { get; set; }
    
    //FK
    public int IdProjectQuotes { get; set; }
    public ProjectQuotes ProjectQuotes { get; set; } = null!;
    
    public int IdVisits { get; set; }
    public Visits Visits { get; set; } = null!;
    
    public ICollection<ListOfCategories> ListOfCategories { get; set; } = new List<ListOfCategories>();


}

public class Appointment
{
    public int Index { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime Datetime { get; set; }
    public string? Notes { get; set; }
    public string? Address { get; set; }
    public string? Reference { get; set; }
}

