using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Common.DTOs.ProductPropertiesDTOs;

public class CreateProductPropertiesDto
{
    public string? CustomValue { get; set; }
    public int IdProduct { get; set; }
    public int IdPropertyValue { get; set; }
}