using AVASphere.ApplicationCore.Common.Entities.Jsons;
using AVASphere.ApplicationCore.Common.Entities.General;

namespace AVASphere.ApplicationCore.Sales.DTOs;

/// <summary>
/// DTO para la respuesta de GetQuotations que incluye los datos del cliente
/// </summary>
public class GetQuotationResponseDto
{
    public int IdQuotation { get; set; }
    public int Folio { get; set; }
    public DateOnly SaleDate { get; set; }
    public StatusEnum Status { get; set; }
    public string? GeneralComment { get; set; }
    
    // Datos del cliente completos
    public CustomerInQuotationDto Customer { get; set; } = new();
    
    // Lista de ejecutivos de venta
    public List<string> SalesExecutives { get; set; } = new();
    
    // Followups
    public List<QuotationFollowupResponseDto> Followups { get; set; } = new();
    
    // Productos
    public List<SingleProductJson>? Products { get; set; }
    
    // Información de venta vinculada
    public string? LinkedSaleId { get; set; }
    public string? LinkedSaleFolio { get; set; }
    public bool IsLinkedToSale { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Configuración del sistema
    public int IdConfigSys { get; set; }
}

/// <summary>
/// Datos del cliente incluidos en la cotización
/// </summary>
public class CustomerInQuotationDto
{
    public int IdCustomer { get; set; }
    public int ExternalId { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string PhoneNumber { get; set; } = "+00";
    public string? Email { get; set; }
    public string? TaxId { get; set; }
    
    // JSON data
    public DirectionJson? Direction { get; set; }
    public SettingsCustomerJson? Settings { get; set; }
    public PaymentMethodsJson? PaymentMethods { get; set; }
    public PaymentTermsJson? PaymentTerms { get; set; }
    
    // Propiedad calculada para nombre completo
    public string FullName => $"{Name} {LastName}".Trim();
}

/// <summary>
/// Followup de cotización para respuesta
/// </summary>
public class QuotationFollowupResponseDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

