using AVASphere.ApplicationCore.Inventory.DTOs;

namespace AVASphere.ApplicationCore.Inventory.Interfaces;

/// <summary>
/// Servicio para manejar operaciones de inventario físico
/// </summary>
public interface IPhysicalInventoryDetailService
{
    /// <summary>
    /// Actualiza la cantidad física de un detalle específico
    /// </summary>
    Task<PhysicalInventoryDetailResponseDto?> UpdatePhysicalQuantityAsync(PhysicalQuantityUpdateDto updateDto);
    
    /// <summary>
    /// Crea un nuevo registro de detalle de inventario físico
    /// </summary>
    Task<PhysicalInventoryDetailResponseDto> CreatePhysicalInventoryDetailAsync(PhysicalInventoryDetailCreateDto createDto);
    
    /// <summary>
    /// Obtiene un detalle específico por su ID
    /// </summary>
    Task<PhysicalInventoryDetailResponseDto?> GetPhysicalInventoryDetailByIdAsync(int id);
    
    /// <summary>
    /// Obtiene todos los detalles de un inventario físico específico
    /// </summary>
    Task<IEnumerable<PhysicalInventoryDetailResponseDto>> GetPhysicalInventoryDetailsByInventoryIdAsync(int idPhysicalInventory);
    
    /// <summary>
    /// Calcula y actualiza automáticamente las diferencias para un inventario completo
    /// </summary>
    Task<bool> RecalculateDifferencesAsync(int idPhysicalInventory);
}
