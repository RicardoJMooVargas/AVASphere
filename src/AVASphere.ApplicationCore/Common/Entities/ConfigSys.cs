namespace AVASphere.ApplicationCore.Common.Entities;

public class ConfigSys
{
    public int Id { get; set; }

    public string CompanyName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = "#000000";
    public string SecondaryColor { get; set; } = "#FFFFFF";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}