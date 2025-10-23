using System.ComponentModel.DataAnnotations;
using AVASphere.ApplicationCore.Common.Attributes;

namespace AVASphere.ApplicationCore.Sales.DTOs;

public class CreateQuotationDto
{
    [Required]
    public int Folio { get; set; }

    public DateTime? SaleDate { get; set; }

    public string? GeneralComment { get; set; }

    [Required]
    public CustomerDto Customer { get; set; } = new();

    public List<QuotationFollowupDto>? Followups { get; set; }
}

public class CustomerDto
{
    public string? CustomerId { get; set; } // Si tiene ID, se editará; si no, se creará nuevo

    public string? Code { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    [OptionalEmail]
    public string? Email { get; set; }

    public List<string>? Phones { get; set; }
}

public class QuotationFollowupDto
{
    public DateTime? Date { get; set; }

    [Required]
    public string Comment { get; set; } = string.Empty;

    public string? UserId { get; set; }
}
