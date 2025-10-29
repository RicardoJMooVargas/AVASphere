using AVASphere.ApplicationCore.Common.Entities.Jsons;
using AVASphere.ApplicationCore.Common.Entities.Products;

namespace AVASphere.ApplicationCore.Common.Entities.General;
// CATALOGO DE PROVEEDORES
public class Supplier
{
    public int IdSupplier { get; set; }
    public string? Name { get; set; }
    public string? CompanyName { get; set; }
    public string? TaxId { get; set; } // RFC O EQUIVALENTE
    public string? PersonType { get; set;}
    public string? BusinessId { get; set; }
    public string? CurrencyCoin { get; set; } // es un enum de tipo de moneda MXN, USD, EUR, ETC.
    public double DeliveryDays { get; set; } // TOTAL DE DIAS DE ENTREGA SUPUESTO
    public DateTime RegistrationDate { get; set;}
    public string? Observations { get; set; }
    
    // RELACIONES
    public ICollection<Product> Product { get; set; } = new List<Product>();
    
    // JSON
    public ContactsJson? ContactsJson { get; set; } 
    public PaymentTermsJson? PaymentTermsJson { get; set; }
    public PaymentMethodsJson? PaymentMethodsJson { get; set; } // NO USAR POR EL MOMENTO
    
}

public class ContactsJson
{
    public int Index { get; set; }
    public string? WebPage { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}


