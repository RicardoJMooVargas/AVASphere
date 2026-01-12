using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.ApplicationCore.Common.Entities.Jsons;
using System.Linq.Expressions;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface ISupplierRepository
{
    // Obtener Suppliers con filtros flexibles
    Task<IEnumerable<Supplier>> GetSuppliersAsync(
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
        bool includeProducts = false);

    // Obtener un Supplier por ID
    Task<Supplier?> GetSupplierByIdAsync(int id, bool includeProducts = false);

    // Búsqueda por datos JSON
    Task<IEnumerable<Supplier>> SearchByContactsAsync(string? webPage = null, string? phoneNumber = null, string? email = null);
    Task<IEnumerable<Supplier>> SearchByPaymentTermsAsync(string? paymentType = null, string? typeOfCurrency = null, DateTime? expirationDateFrom = null, DateTime? expirationDateTo = null);
    Task<IEnumerable<Supplier>> SearchByPaymentMethodsAsync(string? code = null, string? description = null, string? bank = null, string? currency = null);

    // CRUD básico
    Task<Supplier> CreateSupplierAsync(Supplier supplier);
    Task<Supplier?> UpdateSupplierAsync(int id, Supplier supplier);
    Task<bool> DeleteSupplierAsync(int id);

    // Edición de datos JSON por separado
    Task<Supplier?> UpdateContactsJsonAsync(int id, ContactsJson? contactsJson);
    Task<Supplier?> UpdatePaymentTermsJsonAsync(int id, PaymentTermsJson? paymentTermsJson);
    Task<Supplier?> UpdatePaymentMethodsJsonAsync(int id, PaymentMethodsJson? paymentMethodsJson);

    // Verificaciones
    Task<bool> HasRelatedProductsAsync(int supplierId);
    Task<bool> ExistsAsync(int id);
    
    // Búsqueda genérica con expresiones
    Task<IEnumerable<Supplier>> FindAsync(Expression<Func<Supplier, bool>> predicate, bool includeProducts = false);
}