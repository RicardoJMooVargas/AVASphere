using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Extensions;
using AVASphere.ApplicationCore.Common.Interfaces;

namespace AVASphere.Infrastructure.Common.Services;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _supplierRepository;

    public SupplierService(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<SupplierResponseDto> CreateSupplierAsync(CreateSupplierDto createDto)
    {
        // Validaciones de negocio
        if (string.IsNullOrEmpty(createDto.Name))
            throw new ArgumentException("El nombre del proveedor es requerido.", nameof(createDto.Name));

        var supplier = createDto.ToEntity();
        var createdSupplier = await _supplierRepository.CreateSupplierAsync(supplier);
        return createdSupplier.ToResponseDto();
    }

    public async Task<SupplierResponseDto?> GetSupplierByIdAsync(int id, bool includeProducts = false)
    {
        var supplier = await _supplierRepository.GetSupplierByIdAsync(id, includeProducts);
        return supplier?.ToResponseDto();
    }

    public async Task<SupplierResponseDto?> UpdateSupplierAsync(int id, UpdateSupplierDto updateDto)
    {
        // Validaciones de negocio
        if (!await _supplierRepository.ExistsAsync(id))
            return null;

        if (string.IsNullOrEmpty(updateDto.Name))
            throw new ArgumentException("El nombre del proveedor es requerido.", nameof(updateDto.Name));

        // Obtener la entidad existente
        var existingSupplier = await _supplierRepository.GetSupplierByIdAsync(id);
        if (existingSupplier == null)
            return null;

        // Actualizar las propiedades
        existingSupplier.UpdateEntity(updateDto);

        var updatedSupplier = await _supplierRepository.UpdateSupplierAsync(id, existingSupplier);
        return updatedSupplier?.ToResponseDto();
    }

    public async Task<bool> DeleteSupplierAsync(int id)
    {
        // Verificar si existe
        if (!await _supplierRepository.ExistsAsync(id))
            return false;

        // Verificar si tiene productos relacionados
        if (await _supplierRepository.HasRelatedProductsAsync(id))
            throw new InvalidOperationException("No se puede eliminar el proveedor porque tiene productos relacionados.");

        return await _supplierRepository.DeleteSupplierAsync(id);
    }

    public async Task<IEnumerable<SupplierBasicDto>> GetSuppliersBasicAsync()
    {
        var suppliers = await _supplierRepository.GetSuppliersAsync();
        return suppliers.ToBasicDtos();
    }

    public async Task<bool> HasRelatedProductsAsync(int supplierId)
    {
        return await _supplierRepository.HasRelatedProductsAsync(supplierId);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _supplierRepository.ExistsAsync(id);
    }
}
