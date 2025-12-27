using AVASphere.ApplicationCore.Sales.DTOs;

namespace AVASphere.ApplicationCore.Sales.Interfaces;

/// <summary>
/// Interfaz para servicio que combina datos de ventas externas (InforAVA) 
/// con información interna de AVASphere.
/// 
/// MOTIVO:
/// 1. La lógica de NEGOCIO (combinar datos) debe estar en el servicio, no en el repositorio.
/// 2. El repositorio solo obtiene datos, el servicio los procesa y enriquece.
/// 3. Permite cambiar la lógica de combinación sin tocar el acceso a datos.
/// </summary>
public interface IExternalSalesService
{
    /// <summary>
    /// Obtiene ventas del sistema externo InforAVA con filtros avanzados.
    /// 
    /// PROCESO:
    /// 1. Consultar API externa InforAVA
    /// 2. Para cada venta externa, buscar si existe en el sistema interno
    /// 3. Combinar ambas informaciones
    /// 4. Aplicar filtros especificados
    /// 5. Retornar lista paginada
    /// 
    /// FILTROS DISPONIBLES:
    /// - Search: Búsqueda inteligente (números → folio; texto → cliente)
    /// - CustomerName: Filtro por nombre del cliente
    /// - Folio: Filtro por folio
    /// - IsLinked: Filtrar por estado de vinculación
    /// - MinAmount/MaxAmount: Rango de montos
    /// - SatisfactionLevel: Nivel de satisfacción
    /// - Limit/Offset: Paginación
    /// 
    /// MOTIVO: Vista unificada con búsquedas complejas sin múltiples endpoints.
    /// </summary>
    /// <param name="filter">Filtros y parámetros de búsqueda.</param>
    /// <returns>Lista filtrada y paginada de ventas combinadas.</returns>
    Task<IEnumerable<CombinedSalesDto>> GetExternalSalesWithInternalDataAsync(SaleFilterDto? filter = null);

    /// <summary>
    /// Verifica que el sistema externo está disponible.
    /// 
    /// MOTIVO: Health check para monitoreo y alertas.
    /// </summary>
    Task<bool> VerifyExternalConnectionAsync();

    /// <summary>
    /// Obtiene los detalles de productos/movimientos de una venta específica en el sistema externo.
    /// 
    /// PROCESO:
    /// 1. Consultar API externa InforAVA (endpoint DetalleVentaV)
    /// 2. Parsear respuesta y mapear a ExternalSaleDetailDto
    /// 3. Retornar lista de detalles
    /// 
    /// MOTIVO: Ver cada producto vendido (cantidad, precio, descuentos, impuestos) en una venta específica.
    /// </summary>
    /// <returns>Lista de detalles de productos en esa venta.</returns>
    Task<IEnumerable<ExternalSaleDetailDto>> GetSaleDetailAsync(string nf, string caja, string serie, string folio);
}
