using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Inventory.DTOs;

namespace AVASphere.ApplicationCore.Inventory.Interfaces;

public interface IPhysicalInventoryService
{
    /// <summary>
    /// Crea un nuevo registro de Conteo Físico
    /// </summary>
    /// <param name="createDto">Datos para crear el conteo físico</param>
    /// <returns>Respuesta con el conteo físico creado</returns>
    Task<ApiResponse<PhysicalInventoryResponseDto>> CreatePhysicalInventoryAsync(CreatePhysicalInventoryDto createDto, int userId);
    
    /// <summary>
    /// Actualiza un Conteo Físico existente
    /// Condiciones: Solo se permite editar IdWarehouse si no existen registros en PhysicalInventoryDetail
    /// </summary>
    /// <param name="updateDto">Datos para actualizar el conteo físico</param>
    /// <returns>Respuesta con el conteo físico actualizado</returns>
    Task<ApiResponse<PhysicalInventoryResponseDto>> UpdatePhysicalInventoryAsync(UpdatePhysicalInventoryDto updateDto);
    
    /// <summary>
    /// Elimina un Conteo Físico
    /// </summary>
    /// <param name="idPhysicalInventory">ID del conteo físico a eliminar</param>
    /// <param name="forceDelete">Si true, permite eliminación en cascada aunque existan detalles</param>
    /// <returns>Respuesta indicando el resultado de la eliminación</returns>
    Task<ApiResponse<bool>> DeletePhysicalInventoryAsync(int idPhysicalInventory, bool forceDelete = false);
    
    /// <summary>
    /// Obtiene un conteo físico con todos los productos relacionados al warehouse y área del usuario
    /// </summary>
    /// <param name="idPhysicalInventory">ID del conteo físico</param>
    /// <param name="userId">ID del usuario que realiza el conteo (para obtener su área)</param>
    /// <returns>Conteo físico con productos relacionados</returns>
    Task<ApiResponse<PhysicalInventoryWithProductsDto>> GetPhysicalInventoryWithProductsAsync(int idPhysicalInventory, int userId);
    
    /// <summary>
    /// Obtiene un conteo físico por ID
    /// </summary>
    /// <param name="idPhysicalInventory">ID del conteo físico</param>
    /// <returns>Conteo físico encontrado</returns>
    Task<ApiResponse<PhysicalInventoryResponseDto>> GetPhysicalInventoryByIdAsync(int idPhysicalInventory);
    
    /// <summary>
    /// Obtiene todos los conteos físicos con filtros opcionales
    /// </summary>
    /// <param name="idWarehouse">Filtro por warehouse</param>
    /// <param name="status">Filtro por estado</param>
    /// <param name="startDate">Fecha de inicio</param>
    /// <param name="endDate">Fecha de fin</param>
    /// <returns>Lista de conteos físicos</returns>
    Task<ApiResponse<IEnumerable<PhysicalInventoryResponseDto>>> GetPhysicalInventoriesAsync(
        int? idWarehouse = null, 
        string? status = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null);
        
    /// <summary>
    /// Obtener lista de productos para conteo físico basado en IdWarehouse y área del usuario
    /// Filtra por IdWarehouse e IdLocationDetails.IdArea del usuario.
    /// Si no existen registros en Inventory, obtiene productos directamente de la tabla Product.
    /// </summary>
    /// <param name="idWarehouse">ID del warehouse</param>
    /// <param name="userId">ID del usuario (obtenido del token) para determinar su área</param>
    /// <returns>Lista de productos para conteo físico</returns>
    Task<ApiResponse<ProductInventoryListResponseDto>> GetProductInventoryListAsync(int idWarehouse, int userId);
}

