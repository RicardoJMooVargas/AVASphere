namespace VYAACentralInforApi.WebApi.Sale.DTOs;

public class QuotationResponseDto
{
    public string QuotationId { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> SalesExecutives { get; set; } = new();
    public int Folio { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public CustomerResponseDto? Customer { get; set; }
    public string? GeneralComment { get; set; }
    public List<QuotationFollowupResponseDto> Followups { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CustomerResponseDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public List<string>? Phones { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Status { get; set; }
}

public class QuotationFollowupResponseDto
{
    public string Id { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class GetQuotationsQueryDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? CustomerName { get; set; }
    public int? Folio { get; set; }
}
