namespace AVASphere.ApplicationCore.Common.Entities.Jsons;

public class PaymentMethodsJson
{
    public int Index { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Bank { get; set; }
    public int AccountNumber { get; set; }
    public string? ReferencePayment { get; set; }
    public string? Currency { get; set; }
}