using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using System.ComponentModel.DataAnnotations;

namespace AVASphere.ApplicationCore.Common.DTOs;

// DTO para respuesta con datos completos del proveedor
public class SupplierResponseDto
{
    public int IdSupplier { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? TaxId { get; set; }
    public string? PersonType { get; set; }
    public string? BusinessId { get; set; }
    public string? CurrencyCoin { get; set; }
    public double? DeliveryDays { get; set; }
    public DateOnly RegistrationDate { get; set; }
    public string? Observations { get; set; }
    public ContactsJson? ContactsJson { get; set; }
    public PaymentTermsJson? PaymentTermsJson { get; set; }
    public PaymentMethodsJson? PaymentMethodsJson { get; set; }
    public List<ProductBasicDto>? Products { get; set; }
}

// DTO básico para mostrar información de productos en respuesta de proveedor
public class ProductBasicDto
{
    public int IdProduct { get; set; }
    public string MainName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// DTO para crear un nuevo proveedor
public class CreateSupplierDto
{
    [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(150, ErrorMessage = "El nombre de la empresa no puede exceder los 150 caracteres")]
    public string? CompanyName { get; set; }

    [StringLength(50, ErrorMessage = "El Tax ID no puede exceder los 50 caracteres")]
    public string? TaxId { get; set; }

    [StringLength(50, ErrorMessage = "El tipo de persona no puede exceder los 50 caracteres")]
    public string? PersonType { get; set; }

    [StringLength(50, ErrorMessage = "El Business ID no puede exceder los 50 caracteres")]
    public string? BusinessId { get; set; }

    [StringLength(10, ErrorMessage = "La moneda no puede exceder los 10 caracteres")]
    public string? CurrencyCoin { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Los días de entrega deben ser un valor positivo")]
    public double? DeliveryDays { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder los 500 caracteres")]
    public string? Observations { get; set; }

    public ContactsJson? ContactsJson { get; set; }
    public PaymentTermsJson? PaymentTermsJson { get; set; }
    public PaymentMethodsJson? PaymentMethodsJson { get; set; }
}

// DTO para actualizar un proveedor (sin JSONs)
public class UpdateSupplierDto
{
    [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(150, ErrorMessage = "El nombre de la empresa no puede exceder los 150 caracteres")]
    public string? CompanyName { get; set; }

    [StringLength(50, ErrorMessage = "El Tax ID no puede exceder los 50 caracteres")]
    public string? TaxId { get; set; }

    [StringLength(50, ErrorMessage = "El tipo de persona no puede exceder los 50 caracteres")]
    public string? PersonType { get; set; }

    [StringLength(50, ErrorMessage = "El Business ID no puede exceder los 50 caracteres")]
    public string? BusinessId { get; set; }

    [StringLength(10, ErrorMessage = "La moneda no puede exceder los 10 caracteres")]
    public string? CurrencyCoin { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Los días de entrega deben ser un valor positivo")]
    public double? DeliveryDays { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder los 500 caracteres")]
    public string? Observations { get; set; }
}

// DTO para filtros de búsqueda
public class SupplierFilterDto
{
    public string? Name { get; set; }
    public string? CompanyName { get; set; }
    public string? TaxId { get; set; }
    public string? PersonType { get; set; }
    public string? BusinessId { get; set; }
    public string? CurrencyCoin { get; set; }
    public double? MinDeliveryDays { get; set; }
    public double? MaxDeliveryDays { get; set; }
    public DateOnly? RegistrationDateFrom { get; set; }
    public DateOnly? RegistrationDateTo { get; set; }
    public string? Observations { get; set; }
    public int? ProductId { get; set; }
    public bool IncludeProducts { get; set; } = false;
}

// DTO para búsqueda por contactos
public class ContactSearchDto
{
    public string? WebPage { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}

// DTO para búsqueda por términos de pago
public class PaymentTermsSearchDto
{
    public string? PaymentType { get; set; }
    public string? TypeOfCurrency { get; set; }
    public DateTime? ExpirationDateFrom { get; set; }
    public DateTime? ExpirationDateTo { get; set; }
}

// DTO para búsqueda por métodos de pago
public class PaymentMethodsSearchDto
{
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Bank { get; set; }
    public string? Currency { get; set; }
}

// DTO para actualizar contactos
public class UpdateContactsDto
{
    public ContactsJson? ContactsJson { get; set; }
}

// DTO para actualizar términos de pago
public class UpdatePaymentTermsDto
{
    public PaymentTermsJson? PaymentTermsJson { get; set; }
}

// DTO para actualizar métodos de pago
public class UpdatePaymentMethodsDto
{
    public PaymentMethodsJson? PaymentMethodsJson { get; set; }
}

// DTO básico para respuesta simple
public class SupplierBasicDto
{
    public int IdSupplier { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? TaxId { get; set; }
    public string? CurrencyCoin { get; set; }
    public DateOnly RegistrationDate { get; set; }
}