using AVASphere.ApplicationCore.Common.Entities.Jsons;

namespace AVASphere.ApplicationCore.Common.Entities.General;

public class BranchOffice
{
    public int IdBranchOffice { get; set; }
    public string Name { get; set; } = null!;
    public DirectionJson DirectionJson { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    
    // RELACIÓN 1-N
    public ICollection<ConfigSys> ConfigSys { get; set; } = new List<ConfigSys>();
}