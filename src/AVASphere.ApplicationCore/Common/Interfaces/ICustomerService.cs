using AVASphere.ApplicationCore.Common.DTOs;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface ICustomerService
{
    // 1) Obtener datos con filtros opcionales (IdCustomer, LastName, ExternalId)
    Task<IEnumerable<CustomerDto>> GetAsync(CustomerFilterDto? filters);

    // 2) Crear un cliente (permite enviar los 4 JSON u omitirlos)
    Task<CustomerDto> NewAsync(CustomerCreateRequest request);

    // 3) Actualizar selectiva/múltiple: solo se modifican campos no nulos. No se aceptan estructuras JSON distintas.
    Task<CustomerDto> EditAsync(CustomerUpdateRequest request);

    // 4) Eliminar un cliente por Id
    Task<bool> DeleteAsync(int idCustomer);

    // 5) Búsqueda inteligente por nombre completo
    Task<IEnumerable<CustomerDto>> SearchAsync(string searchText);

    // 6) Importar clientes desde archivo Excel
    Task<CustomerImportResultDto> ImportFromExcelAsync(Stream excelFileStream);

    // 7) Restablecer tabla Customers
    Task<bool> ResetTableAsync();
}