namespace AVASphere.ApplicationCore.Projects.Entities;

public class Visits
{
    public int IdVisits { get; set; }
    public string? Image { get; set; }
    public string? Description { get; set; }
    
    //FK
    public ICollection<Projects> Projects { get; set; } = new List<Projects>();
}