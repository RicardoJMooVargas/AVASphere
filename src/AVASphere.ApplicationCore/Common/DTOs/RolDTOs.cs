namespace AVASphere.ApplicationCore.Common.DTOs;


public class RolRequestDto
{
    public string Name { get; set; } = null!;
    public string? NormalizedName { get; set; }
    public int IdArea { get; set; }
}

public class RolResponseDto
{
    public int IdRol { get; set; }
    public string Name { get; set; } = null!;
    public string? NormalizedName { get; set; }
    public int IdArea { get; set; }
    public string AreaName { get; set; } = null!;
    public int UserCount { get; set; }
}