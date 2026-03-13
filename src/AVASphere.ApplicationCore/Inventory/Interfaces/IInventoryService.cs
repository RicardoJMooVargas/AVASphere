using AVASphere.ApplicationCore.Inventory.DTOs;

namespace AVASphere.ApplicationCore.Inventory.Interfaces;

/// <summary>
/// Servicio para gestión de inventario
/// </summary>
public interface IInventoryService
{
    /// <summary>
    /// Importa inventario desde un archivo Excel
    /// </summary>
    /// <param name="excelStream">Stream del archivo Excel</param>
    /// <returns>Resultado de la importación con estadísticas y errores</returns>
    Task<ImportInventoryResultDto> ImportInventoryFromExcelAsync(Stream excelStream);

    /// <summary>
    /// Importa inventario de ubicación desde un archivo Excel
    /// </summary>
    /// <param name="excelStream">Stream del archivo Excel</param>
    /// <returns>Resultado de la importación con estadísticas y errores</returns>
    Task<ImportInventoryResultDto> ImportInventoryUbicationFromExcelAsync(Stream excelStream);

    /// <summary>
    /// Crea un nuevo registro de inventario
    /// </summary>
    /// <param name="createDto">Datos del inventario</param>
    /// <returns>Inventario creado</returns>
    Task<InventoryResponseDto> CreateInventoryAsync(CreateInventoryDto createDto);

    /// <summary>
    /// Obtiene el inventario por ID
    /// </summary>
    /// <param name="idInventory">ID del inventario</param>
    /// <returns>Inventario o null si no existe</returns>
    Task<InventoryResponseDto?> GetInventoryByIdAsync(int idInventory);

    /// <summary>
    /// Obtiene todo el inventario con filtros opcionales y paginación
    /// </summary>
    /// <param name="pageNumber">Número de página (base 1, por defecto 1)</param>
    /// <param name="pageSize">Tamaño de página (por defecto 20, máximo 10000)</param>
    /// <param name="idInventory">Filtrar por ID de inventario (opcional)</param>
    /// <param name="idWarehouse">Filtrar por ID de bodega (opcional)</param>
    /// <param name="warehouseCode">Filtrar por código de bodega (AVA01, AVA02, etc.) (opcional)</param>
    /// <param name="idProduct">Filtrar por ID de producto (opcional)</param>
    /// <param name="productName">Buscar por nombre/descripción del producto (opcional)</param>
    /// <returns>Respuesta paginada con inventario filtrado y metadatos de paginación</returns>
    Task<PaginatedInventoryResponseDto> GetAllInventoryAsync(
        int pageNumber = 1,
        int pageSize = 20,
        int? idInventory = null,
        int? idWarehouse = null,
        string? warehouseCode = null,
        int? idProduct = null,
        string? productName = null);
}
