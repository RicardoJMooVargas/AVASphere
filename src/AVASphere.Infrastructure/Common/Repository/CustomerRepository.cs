using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Common.Repository;

public class CustomerRepository : ICustomerRepository
{
    private readonly MasterDbContext _context;

    public CustomerRepository(MasterDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Customer>> SelectAsync(int? idCustomer, string? lastName, int? externalId)
    {
        var query = _context.Customers.AsQueryable();

        if (idCustomer.HasValue)
            query = query.Where(c => c.IdCustomer == idCustomer.Value);

        if (!string.IsNullOrWhiteSpace(lastName))
            query = query.Where(c => c.LastName != null && EF.Functions.ILike(c.LastName, $"%{lastName}%"));

        if (externalId.HasValue)
            query = query.Where(c => c.ExternalId == externalId.Value);

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<Customer> InsertAsync(Customer customer)
    {
        // Asegurar JSON requeridos
        if (customer.DirectionJson is null)
        {
            customer.DirectionJson = new();
        }

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> UpdateAsync(Customer customer)
    {
        var existing = await _context.Customers.FirstOrDefaultAsync(c => c.IdCustomer == customer.IdCustomer);
        if (existing == null)
            throw new KeyNotFoundException($"Customer with Id {customer.IdCustomer} not found.");

        // Actualización de campos (reemplazo completo en repositorio; actualización parcial debe manejarse en el servicio)
        existing.ExternalId = customer.ExternalId;
        existing.Name = customer.Name;
        existing.LastName = customer.LastName;
        existing.PhoneNumber = customer.PhoneNumber;
        existing.Email = customer.Email;
        existing.TaxId = customer.TaxId;
        existing.SettingsCustomerJson = customer.SettingsCustomerJson;
        if (customer.DirectionJson is not null)
        {
            existing.DirectionJson = customer.DirectionJson; // requerido
        }
        existing.PaymentMethodsJson = customer.PaymentMethodsJson;
        existing.PaymentTermsJson = customer.PaymentTermsJson;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int idCustomer)
    {
        var existing = await _context.Customers.FindAsync(idCustomer);
        if (existing == null) return false;

        _context.Customers.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetNextIndexForSettingsAsync()
    {
        // Traer datos a memoria y luego procesar (EF Core no puede traducir propiedades JSON anidadas)
        var customers = await _context.Customers
            .Where(c => c.SettingsCustomerJson != null)
            .AsNoTracking()
            .ToListAsync();
        
        var maxIndex = customers
            .Where(c => c.SettingsCustomerJson != null)
            .Select(c => c.SettingsCustomerJson!.Index)
            .DefaultIfEmpty(0)
            .Max();
        
        return maxIndex + 1;
    }

    public async Task<int> GetNextIndexForDirectionAsync()
    {
        // DirectionJson es requerido, siempre existe - procesar en memoria
        var customers = await _context.Customers
            .AsNoTracking()
            .ToListAsync();
        
        var maxIndex = customers
            .Select(c => c.DirectionJson.Index)
            .DefaultIfEmpty(0)
            .Max();
        
        return maxIndex + 1;
    }

    public async Task<int> GetNextIndexForPaymentMethodsAsync()
    {
        // Traer datos a memoria y luego procesar
        var customers = await _context.Customers
            .Where(c => c.PaymentMethodsJson != null)
            .AsNoTracking()
            .ToListAsync();
        
        var maxIndex = customers
            .Where(c => c.PaymentMethodsJson != null)
            .Select(c => c.PaymentMethodsJson!.Index)
            .DefaultIfEmpty(0)
            .Max();
        
        return maxIndex + 1;
    }

    public async Task<int> GetNextIndexForPaymentTermsAsync()
    {
        // Traer datos a memoria y luego procesar
        var customers = await _context.Customers
            .Where(c => c.PaymentTermsJson != null)
            .AsNoTracking()
            .ToListAsync();
        
        var maxIndex = customers
            .Where(c => c.PaymentTermsJson != null)
            .Select(c => c.PaymentTermsJson!.Index)
            .DefaultIfEmpty(0)
            .Max();
        
        return maxIndex + 1;
    }
}