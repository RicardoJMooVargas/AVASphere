namespace AVASphere.ApplicationCore.Common.DTOs.ProductPropertiesDTOs;

public class ProductPropertiesResponseDto
{
    public int IdProductProperties { get; set; }
    public string? CustomValue { get; set; }
    public int IdProduct { get; set; }
    public int IdPropertyValue { get; set; }

    // Información relacionada (opcional)
    public string? ProductName { get; set; }
    public string? PropertyValueName { get; set; }
}