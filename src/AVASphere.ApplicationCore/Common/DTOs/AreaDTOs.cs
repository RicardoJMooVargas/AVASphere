namespace AVASphere.ApplicationCore.Common.DTOs;

public class AreaRequestDto
{
    public string Name { get; set; } = null!;
    public string? NormalizedName { get; set; }
}

public class AreaResponseDto
{
    public int IdArea { get; set; }
    public string Name { get; set; } = null!;
    public string? NormalizedName { get; set; }
    public int RolCount { get; set; }
}