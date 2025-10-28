namespace AVASphere.ApplicationCore.Common.Entities;

public class Supplier
{
    public int IdSupplier { get; set; }
    public string? Name { get; set; }
    public string? CompanyName { get; set; }
    public string? Rfc { get; set; }
    public string? PersonType { get; set;}
    public string? Curp { get; set; }
    public string? Coin { get; set; }
    public double DeliveryDays { get; set; }
    public DateTime RegistrationDate { get; set;}
    public string? Observations { get; set; }
    
    // FK
    public ICollection<Product> Product { get; set; } = new List<Product>();
    
    // JSON
    public ContactsJson? ContactsJson { get; set; }
    public PaymentTerms? PaymentTerms { get; set; }
    public PaymentMethods? PaymentMethods { get; set; }
}

public class ContactsJson
{
    public int Index { get; set; }
    public string? Name { get; set; }
    public double PhoneNumber { get; set; }
}

public class PaymentTerms
{
    public int Index { get; set; }
    public string? PaymentType { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string? TypeOfCurrency { get; set;}
}

public class PaymentMethods
{
    public int Index { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Bank { get; set; }
    public int AccountNumber { get; set; }
    public string? ReferencePayment { get; set; }
    public string? Currency { get; set; }
}