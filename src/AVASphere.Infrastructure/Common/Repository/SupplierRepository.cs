using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using AVASphere.ApplicationCore.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AVASphere.Infrastructure.Common.Repository;

public class SupplierRepository : ISupplierRepository
{
    private readonly MasterDbContext _context;

    public SupplierRepository(MasterDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Supplier>> GetSuppliersAsync(
        string? name = null,
        string? companyName = null,
        string? taxId = null,
        string? personType = null,
        string? businessId = null,
        string? currencyCoin = null,
        double? minDeliveryDays = null,
        double? maxDeliveryDays = null,
        DateOnly? registrationDateFrom = null,
        DateOnly? registrationDateTo = null,
        string? observations = null,
        int? productId = null,
        bool includeProducts = false)
    {
        var query = _context.Suppliers.AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrEmpty(name))
            query = query.Where(s => s.Name.Contains(name));

        if (!string.IsNullOrEmpty(companyName))
            query = query.Where(s => s.CompanyName != null && s.CompanyName.Contains(companyName));

        if (!string.IsNullOrEmpty(taxId))
            query = query.Where(s => s.TaxId != null && s.TaxId.Contains(taxId));

        if (!string.IsNullOrEmpty(personType))
            query = query.Where(s => s.PersonType != null && s.PersonType.Contains(personType));

        if (!string.IsNullOrEmpty(businessId))
            query = query.Where(s => s.BusinessId != null && s.BusinessId.Contains(businessId));

        if (!string.IsNullOrEmpty(currencyCoin))
            query = query.Where(s => s.CurrencyCoin != null && s.CurrencyCoin == currencyCoin);

        if (minDeliveryDays.HasValue)
            query = query.Where(s => s.DeliveryDays >= minDeliveryDays.Value);

        if (maxDeliveryDays.HasValue)
            query = query.Where(s => s.DeliveryDays <= maxDeliveryDays.Value);

        if (registrationDateFrom.HasValue)
            query = query.Where(s => s.RegistrationDate >= registrationDateFrom.Value);

        if (registrationDateTo.HasValue)
            query = query.Where(s => s.RegistrationDate <= registrationDateTo.Value);

        if (!string.IsNullOrEmpty(observations))
            query = query.Where(s => s.Observations != null && s.Observations.Contains(observations));

        if (productId.HasValue)
            query = query.Where(s => s.Product.Any(p => p.IdProduct == productId.Value));

        if (includeProducts)
            query = query.Include(s => s.Product);

        return await query.ToListAsync();
    }

    public async Task<Supplier?> GetSupplierByIdAsync(int id, bool includeProducts = false)
    {
        var query = _context.Suppliers.AsQueryable();

        if (includeProducts)
            query = query.Include(s => s.Product);

        return await query.FirstOrDefaultAsync(s => s.IdSupplier == id);
    }

    public async Task<IEnumerable<Supplier>> SearchByContactsAsync(string? webPage = null, string? phoneNumber = null, string? email = null)
    {
        var query = _context.Suppliers.AsQueryable();

        if (!string.IsNullOrEmpty(webPage))
            query = query.Where(s => s.ContactsJson != null && 
                               EF.Functions.JsonContains(s.ContactsJson, $@"{{""WebPage"":""{webPage}""}}"));

        if (!string.IsNullOrEmpty(phoneNumber))
            query = query.Where(s => s.ContactsJson != null && 
                               EF.Functions.JsonContains(s.ContactsJson, $@"{{""PhoneNumber"":""{phoneNumber}""}}"));

        if (!string.IsNullOrEmpty(email))
            query = query.Where(s => s.ContactsJson != null && 
                               EF.Functions.JsonContains(s.ContactsJson, $@"{{""Email"":""{email}""}}"));

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Supplier>> SearchByPaymentTermsAsync(string? paymentType = null, string? typeOfCurrency = null, DateTime? expirationDateFrom = null, DateTime? expirationDateTo = null)
    {
        var query = _context.Suppliers.AsQueryable();

        if (!string.IsNullOrEmpty(paymentType))
            query = query.Where(s => s.PaymentTermsJson != null && 
                               EF.Functions.JsonContains(s.PaymentTermsJson, $@"{{""PaymentType"":""{paymentType}""}}"));

        if (!string.IsNullOrEmpty(typeOfCurrency))
            query = query.Where(s => s.PaymentTermsJson != null && 
                               EF.Functions.JsonContains(s.PaymentTermsJson, $@"{{""TypeOfCurrency"":""{typeOfCurrency}""}}"));

        // Para fechas en JSON, necesitamos usar búsqueda más específica
        if (expirationDateFrom.HasValue || expirationDateTo.HasValue)
        {
            var suppliers = await query.ToListAsync();
            return suppliers.Where(s => s.PaymentTermsJson != null &&
                                      (!expirationDateFrom.HasValue || s.PaymentTermsJson.ExpirationDate >= expirationDateFrom.Value) &&
                                      (!expirationDateTo.HasValue || s.PaymentTermsJson.ExpirationDate <= expirationDateTo.Value));
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Supplier>> SearchByPaymentMethodsAsync(string? code = null, string? description = null, string? bank = null, string? currency = null)
    {
        var query = _context.Suppliers.AsQueryable();

        if (!string.IsNullOrEmpty(code))
            query = query.Where(s => s.PaymentMethodsJson != null && 
                               EF.Functions.JsonContains(s.PaymentMethodsJson, $@"{{""Code"":""{code}""}}"));

        if (!string.IsNullOrEmpty(description))
            query = query.Where(s => s.PaymentMethodsJson != null && 
                               EF.Functions.JsonContains(s.PaymentMethodsJson, $@"{{""Description"":""{description}""}}"));

        if (!string.IsNullOrEmpty(bank))
            query = query.Where(s => s.PaymentMethodsJson != null && 
                               EF.Functions.JsonContains(s.PaymentMethodsJson, $@"{{""Bank"":""{bank}""}}"));

        if (!string.IsNullOrEmpty(currency))
            query = query.Where(s => s.PaymentMethodsJson != null && 
                               EF.Functions.JsonContains(s.PaymentMethodsJson, $@"{{""Currency"":""{currency}""}}"));

        return await query.ToListAsync();
    }

    public async Task<Supplier> CreateSupplierAsync(Supplier supplier)
    {
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();
        return supplier;
    }

    public async Task<Supplier?> UpdateSupplierAsync(int id, Supplier supplier)
    {
        var existingSupplier = await _context.Suppliers.FindAsync(id);
        if (existingSupplier == null)
            return null;

        // Actualizar propiedades básicas (sin JSONs)
        existingSupplier.Name = supplier.Name;
        existingSupplier.CompanyName = supplier.CompanyName;
        existingSupplier.TaxId = supplier.TaxId;
        existingSupplier.PersonType = supplier.PersonType;
        existingSupplier.BusinessId = supplier.BusinessId;
        existingSupplier.CurrencyCoin = supplier.CurrencyCoin;
        existingSupplier.DeliveryDays = supplier.DeliveryDays;
        existingSupplier.RegistrationDate = supplier.RegistrationDate;
        existingSupplier.Observations = supplier.Observations;

        await _context.SaveChangesAsync();
        return existingSupplier;
    }

    public async Task<bool> DeleteSupplierAsync(int id)
    {
        var supplier = await _context.Suppliers.Include(s => s.Product).FirstOrDefaultAsync(s => s.IdSupplier == id);
        if (supplier == null)
            return false;

        // Verificar si tiene productos relacionados
        if (supplier.Product.Any())
            return false; // No se puede eliminar por integridad referencial

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Supplier?> UpdateContactsJsonAsync(int id, ContactsJson? contactsJson)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null)
            return null;

        supplier.ContactsJson = contactsJson;
        await _context.SaveChangesAsync();
        return supplier;
    }

    public async Task<Supplier?> UpdatePaymentTermsJsonAsync(int id, PaymentTermsJson? paymentTermsJson)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null)
            return null;

        supplier.PaymentTermsJson = paymentTermsJson;
        await _context.SaveChangesAsync();
        return supplier;
    }

    public async Task<Supplier?> UpdatePaymentMethodsJsonAsync(int id, PaymentMethodsJson? paymentMethodsJson)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null)
            return null;

        supplier.PaymentMethodsJson = paymentMethodsJson;
        await _context.SaveChangesAsync();
        return supplier;
    }

    public async Task<bool> HasRelatedProductsAsync(int supplierId)
    {
        return await _context.Products.AnyAsync(p => p.IdSupplier == supplierId);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Suppliers.AnyAsync(s => s.IdSupplier == id);
    }

    public async Task<IEnumerable<Supplier>> FindAsync(Expression<Func<Supplier, bool>> predicate, bool includeProducts = false)
    {
        var query = _context.Suppliers.AsQueryable();

        if (includeProducts)
            query = query.Include(s => s.Product);

        return await query.Where(predicate).ToListAsync();
    }
}