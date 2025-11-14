using AVASphere.ApplicationCore.Common.Entities.General;
namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface ICustomerRepository
{
    // 1) Obtener datos con filtros opcionales (IdCustomer, LastName, ExternalId)
    Task<IEnumerable<Customer>> SelectAsync(int? idCustomer, string? lastName, int? externalId);

    // 2) Crear un cliente
    Task<Customer> InsertAsync(Customer customer);

    // 3) Actualizar un cliente (la lógica de actualización parcial se manejará en el servicio)
    Task<Customer> UpdateAsync(Customer customer);

    // 4) Eliminar un cliente por Id
    Task<bool> DeleteAsync(int idCustomer);
    Task<Customer?> GetByIdAsync(int idCustomer);


    // 5) Métodos para auto-incremento de índices (optimizados)
    Task<int> GetNextIndexForSettingsAsync();
    Task<int> GetNextIndexForDirectionAsync();
    Task<int> GetNextIndexForPaymentMethodsAsync();
    Task<int> GetNextIndexForPaymentTermsAsync();

    // 👇 Nuevo método
    Task<Customer?> FindByNameOrCodeAsync(string clienteCodeOrName);
}