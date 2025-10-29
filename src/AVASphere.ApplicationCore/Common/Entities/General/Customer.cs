using AVASphere.ApplicationCore.Projects.Entities.General;
namespace AVASphere.ApplicationCore.Common.Entities.General;


public class Customer
{
    public int IdCustomer { get; set; }
    public string? Name { get; set;}
    public string? LastName { get; set; }
    public int PhoneNumber { get; set; }
    public string? Email { get; set;}
    
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    
    // JSON
    public DirectionJson DirectionJson { get; set; } = null!;

}

