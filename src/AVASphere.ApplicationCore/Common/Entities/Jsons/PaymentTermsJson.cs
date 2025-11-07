namespace AVASphere.ApplicationCore.Common.Entities.Jsons;

public class PaymentTermsJson
{
    public int Index { get; set; }
    public string? PaymentType { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string? TypeOfCurrency { get; set;}
}
