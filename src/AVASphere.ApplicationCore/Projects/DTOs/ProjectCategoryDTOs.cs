namespace AVASphere.ApplicationCore.Projects.DTOs;

public class ProjectCategoryRequestDto
{
    public string Name { get; set; } = null!;
    public string? NormalizedName { get; set; }
}

public class ProjectCategoryResponseDto
{
    public int IdProjectCategory { get; set; }
    public string Name { get; set; } = null!;
    public string? NormalizedName { get; set; }
}