using AVASphere.ApplicationCore.Common.Entities.Jsons;
namespace AVASphere.ApplicationCore.Common.Entities.General;


public class Customer
{
    public int IdCustomer { get; set; }
    public int ExternalId { get; set; }
    public string? Name { get; set;}
    public string? LastName { get; set; }
    public int PhoneNumber { get; set; }
    public string? Email { get; set;}
    public string? TaxId {get; set;}
    // JSON
    public SettingsCustomerJson? SettingsCustomerJson { get; set; } = null!;
    public DirectionJson DirectionJson { get; set; } = null!;
    public PaymentMethodsJson? PaymentMethodsJson { get; set;} = null!;
    public PaymentTermsJson? PaymentTermsJson { get; set; } = null!;
    // FALTARIAN CFDI Y REGIMEN FISCAL ??
}

public class SettingsCustomerJson
{
    public int Index { get; set; }
    public string? Route { get; set; }
    public string Type { get; set; } = "General"; // aluminiero , general, etc.
    public double Discount { get; set; } = 0.0;
}

