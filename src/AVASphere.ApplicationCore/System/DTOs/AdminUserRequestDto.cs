namespace AVASphere.ApplicationCore.System.DTOs;

public class AdminUserRequestDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}


public class SystemConfigRequestDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public List<ColorJsonDto>? Colors { get; set; }
    public List<int>? NotUseModules { get; set; }
}

public class ColorJsonDto
{
    public int Index { get; set; }
    public string NameColor { get; set; } = string.Empty;
    public string ColorCode { get; set; } = string.Empty;
    public string ColorRgb { get; set; } = string.Empty;
}

