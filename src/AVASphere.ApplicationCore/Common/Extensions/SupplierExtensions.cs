using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.ApplicationCore.Common.Entities.Products;

namespace AVASphere.ApplicationCore.Common.Extensions;

public static class SupplierExtensions
{
    // Mapear de Supplier a SupplierResponseDto
    public static SupplierResponseDto ToResponseDto(this Supplier supplier)
    {
        return new SupplierResponseDto
        {
            IdSupplier = supplier.IdSupplier,
            Name = supplier.Name,
            CompanyName = supplier.CompanyName,
            TaxId = supplier.TaxId,
            PersonType = supplier.PersonType,
            BusinessId = supplier.BusinessId,
            CurrencyCoin = supplier.CurrencyCoin,
            DeliveryDays = supplier.DeliveryDays,
            RegistrationDate = supplier.RegistrationDate,
            Observations = supplier.Observations,
            ContactsJson = supplier.ContactsJson,
            PaymentTermsJson = supplier.PaymentTermsJson,
            PaymentMethodsJson = supplier.PaymentMethodsJson,
            Products = supplier.Product.Select(p => p.ToProductBasicDto()).ToList()
        };
    }

    // Mapear de Supplier a SupplierBasicDto
    public static SupplierBasicDto ToBasicDto(this Supplier supplier)
    {
        return new SupplierBasicDto
        {
            IdSupplier = supplier.IdSupplier,
            Name = supplier.Name,
            CompanyName = supplier.CompanyName,
            TaxId = supplier.TaxId,
            CurrencyCoin = supplier.CurrencyCoin,
            RegistrationDate = supplier.RegistrationDate
        };
    }

    // Mapear de CreateSupplierDto a Supplier
    public static Supplier ToEntity(this CreateSupplierDto dto)
    {
        return new Supplier
        {
            Name = dto.Name,
            CompanyName = dto.CompanyName,
            TaxId = dto.TaxId,
            PersonType = dto.PersonType,
            BusinessId = dto.BusinessId,
            CurrencyCoin = dto.CurrencyCoin,
            DeliveryDays = dto.DeliveryDays,
            RegistrationDate = DateOnly.FromDateTime(DateTime.Now),
            Observations = dto.Observations,
            ContactsJson = dto.ContactsJson,
            PaymentTermsJson = dto.PaymentTermsJson,
            PaymentMethodsJson = dto.PaymentMethodsJson
        };
    }

    // Mapear de UpdateSupplierDto a Supplier (solo propiedades básicas)
    public static void UpdateEntity(this Supplier supplier, UpdateSupplierDto dto)
    {
        supplier.Name = dto.Name;
        supplier.CompanyName = dto.CompanyName;
        supplier.TaxId = dto.TaxId;
        supplier.PersonType = dto.PersonType;
        supplier.BusinessId = dto.BusinessId;
        supplier.CurrencyCoin = dto.CurrencyCoin;
        supplier.DeliveryDays = dto.DeliveryDays;
        supplier.Observations = dto.Observations;
    }

    // Mapear lista de Suppliers a DTOs
    public static IEnumerable<SupplierResponseDto> ToResponseDtos(this IEnumerable<Supplier> suppliers)
    {
        return suppliers.Select(s => s.ToResponseDto());
    }

    // Mapear lista de Suppliers a DTOs básicos
    public static IEnumerable<SupplierBasicDto> ToBasicDtos(this IEnumerable<Supplier> suppliers)
    {
        return suppliers.Select(s => s.ToBasicDto());
    }
}

// Extensión para Product (se necesita para los DTOs de respuesta)
public static class ProductExtensions
{
    public static ProductBasicDto ToProductBasicDto(this Product product)
    {
        return new ProductBasicDto
        {
            IdProduct = product.IdProduct,
            MainName = product.MainName,
            SupplierName = product.SupplierName,
            Unit = product.Unit,
            Price = (decimal)(product.CostsJson?.FirstOrDefault()?.Amount ?? 0)
        };
    }
}
