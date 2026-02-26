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
    /// Obtener lista de productos para conteo físico basado en el IdPhysicalInventory
    /// Obtiene todos los PhysicalInventoryDetail asociados al inventario físico especificado
    /// </summary>
    /// <param name="idPhysicalInventory">ID del inventario físico</param>
    /// <param name="userId">ID del usuario (obtenido del token) para validación</param>
    /// <returns>Lista de productos para conteo físico</returns>
    Task<ApiResponse<ProductInventoryListResponseDto>> GetProductInventoryListAsync(int idPhysicalInventory, int userId);
    
    /// <summary>
    /// Obtener lista paginada de productos para conteo físico con filtros y catálogos
    /// Incluye paginación, filtros por texto, proveedor, familia, clase y línea,
    /// además de las listas de catálogos para filtros en el frontend
    /// </summary>
    /// <param name="idPhysicalInventory">ID del inventario físico</param>
    /// <param name="userId">ID del usuario (obtenido del token) para validación</param>
    /// <param name="pagination">Parámetros de paginación</param>
    /// <param name="filters">Filtros de búsqueda</param>
    /// <returns>Lista paginada de productos con catálogos para filtros</returns>
    Task<ApiResponse<ProductInventoryListPaginatedResponseDto>> GetProductInventoryListPaginatedAsync(
        int idPhysicalInventory, 
        int userId, 
        ProductInventoryListPaginationDto pagination, 
        ProductInventoryListFiltersDto? filters = null);
    
    /// <summary>
    /// Crea o actualiza un conteo específico de un producto en el inventario físico
    /// </summary>
    /// <param name="countDto">Datos del conteo a crear o actualizar</param>
    /// <returns>Respuesta con el detalle del conteo actualizado</returns>
    Task<ApiResponse<PhysicalInventoryCountResponseDto>> CreateOrUpdatePhysicalCountAsync(CreateOrUpdatePhysicalCountDto countDto);
}

