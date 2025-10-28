namespace AVASphere.ApplicationCore.Projects.Entities;
using AVASphere.ApplicationCore.Common.Entities;
public class Visits
{
    public int IdVisits { get; set; }
    public string? Image { get; set; }
    public string? Description { get; set; }
    
    //FK
    public ICollection<Projects> Projects { get; set; } = new List<Projects>();
    
    public int IdCustomer { get; set; }
    public Customer Customer { get; set; } = null!;
    
}