namespace AVASphere.ApplicationCore.Common.DTOs;

public class PropertyRequestDto
{
    public string Name { get; set; } = null!;
    public string? NormalizedName { get; set; }
}

public class PropertyResponseDto
{
    public int IdProperty { get; set; }
    public string Name { get; set; } = null!;
    public string? NormalizedName { get; set; }
}