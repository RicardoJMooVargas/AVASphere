using AVASphere.ApplicationCore.Common.Entities.General;

namespace AVASphere.ApplicationCore.Projects.Entities.General;
using AVASphere.ApplicationCore.Common.Entities;
public class Visit
{
    public int IdVisits { get; set; }
    public string? Image { get; set; }
    public string? Description { get; set; }
    
    //FK
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    
    public int IdCustomer { get; set; }
    public Customer Customer { get; set; } = null!;
}