namespace AVASphere.ApplicationCore.Common.DTOs;

public class PropertyValueRequestDto
{
    public string? Value { get; set; }
    public int IdProperty { get; set; }
}

public class PropertyValueResponseDto
{
    public int IdPropertyValue { get; set; }
    public string? Value { get; set; }
    public string? NameProperty { get; set; }
}

public class PropertyValueFilterDto
{
    public int? IdPropertyValue { get; set; }
    public string? Value { get; set; }
    public string? IdPropertyOrName { get; set; }
}

public class PropertyValueUpdateDto
{
    public string? Value { get; set; }
    public int? IdProperty { get; set; }
}

