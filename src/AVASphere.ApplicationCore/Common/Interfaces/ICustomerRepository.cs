﻿using AVASphere.ApplicationCore.Common.Entities.General;
namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface ICustomerRepository
{
    // 1) Obtener datos con filtros opcionales (IdCustomer, LastName, ExternalId)
    Task<IEnumerable<Customer>> SelectAsync(int? idCustomer, string? lastName, int? externalId);

    // 1.5) Obtener un cliente por ID con tracking habilitado (para actualizaciones)
    Task<Customer?> GetByIdForUpdateAsync(int idCustomer);

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

    // Método para obtener el siguiente ExternalId disponible
    Task<int> GetNextExternalIdAsync();

    // 👇 Nuevo método
    Task<Customer?> FindByNameOrCodeAsync(string clienteCodeOrName);

    // Búsqueda inteligente por coincidencia de texto en nombre completo
    Task<IEnumerable<Customer>> SearchByFullNameAsync(string searchText);

    // Verificar si existe un cliente por ExternalId
    Task<bool> ExistsByExternalIdAsync(int externalId);

    // Restablecer tabla Customers (eliminar todos los registros y reiniciar secuencia)
    Task<bool> ResetTableAsync();

    // Métodos optimizados para importación batch
    Task<List<int>> GetExistingExternalIdsAsync(List<int> externalIds);
    Task<IEnumerable<Customer>> InsertBatchAsync(List<Customer> customers);

    // Búsqueda por ExternalId exacto
    Task<IEnumerable<Customer>> SearchByExternalIdAsync(int externalId);
}