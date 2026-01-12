using AVASphere.ApplicationCore.Common.DTOs;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface ISupplierService
{
    // CRUD básico para catálogos
    Task<SupplierResponseDto> CreateSupplierAsync(CreateSupplierDto createDto);
    Task<SupplierResponseDto?> GetSupplierByIdAsync(int id, bool includeProducts = false);
    Task<SupplierResponseDto?> UpdateSupplierAsync(int id, UpdateSupplierDto updateDto);
    Task<bool> DeleteSupplierAsync(int id);
    
    // Obtener lista básica para catálogo
    Task<IEnumerable<SupplierBasicDto>> GetSuppliersBasicAsync();
    
    // Verificaciones
    Task<bool> HasRelatedProductsAsync(int supplierId);
    Task<bool> ExistsAsync(int id);
}
