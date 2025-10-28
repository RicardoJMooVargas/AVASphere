namespace AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Projects.Entities;

public class Customer
{
    public int IdCustomer { get; set; }
    public string? Name { get; set;}
    public string? LastName { get; set; }
    public int PhoneNumber { get; set; }
    public string? Email { get; set;}
    
    // FK
    public int IdProject { get; set; }
    public Projects Projects { get; set; } = null!;
    
    public ICollection<Visits> Visits { get; set; } = new List<Visits>();
    
    // JSON
    public DirectionJson DirectionJson { get; set; } = null!;

}

