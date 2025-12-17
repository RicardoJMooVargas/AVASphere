using AVASphere.ApplicationCore.Sales.DTOs;

namespace AVASphere.ApplicationCore.Sales.Interfaces;

/// <summary>
/// Interfaz para manejar consultas al sistema externo InforAVA.
/// 
/// MOTIVO: 
/// 1. SEPARACIÓN DE RESPONSABILIDADES: Aislar la lógica de comunicación externa.
/// 2. INYECCIÓN DE DEPENDENCIAS: Permite cambiar la implementación sin afectar el negocio.
/// 3. TESTABILIDAD: Facilita hacer mocks en pruebas unitarias.
/// 4. FLEXIBILIDAD: Si cambiamos de proveedor externo (ej: otro ERP), solo reemplazamos la implementación.
/// </summary>
public interface IExternalSalesRepository
{
    /// <summary>
    /// Consulta ventas del sistema externo InforAVA para una fecha específica y catálogo.
    /// 
    /// MOTIVO: Consumir el endpoint externo de manera centralizada y reutilizable.
    /// </summary>
    /// <param name="catalogo">Identificador del catálogo en el sistema InforAVA (ej: "001", "002").</param>
    /// <param name="fecha">Fecha para filtrar las ventas (formato: YYYY-MM-DD o similar según API).</param>
    /// <returns>Lista de ventas externas encontradas en esa fecha.</returns>
    Task<IEnumerable<ExternalSalesDto>> GetSalesByDateAndCatalogAsync(string catalogo, DateTime fecha);

    /// <summary>
    /// Obtiene una venta específica del sistema externo por su folio/ID.
    /// 
    /// MOTIVO: Permitir búsqueda de una venta individual sin descargar todas del día.
    /// </summary>
    /// <param name="folioExterno">ID o folio de la venta en el sistema externo.</param>
    /// <returns>Los datos de la venta externa o null si no existe.</returns>
    Task<ExternalSalesDto?> GetSaleByFolioAsync(string folioExterno);

    /// <summary>
    /// Verifica la conectividad y disponibilidad del servicio externo.
    /// 
    /// MOTIVO: Implementar health checks para monitores y alarmas.
    /// </summary>
    /// <returns>True si el servicio está disponible, False en caso contrario.</returns>
    Task<bool> IsExternalServiceAvailableAsync();

    /// <summary>
    /// Obtiene los detalles de productos/movimientos de una venta específica en el sistema externo.
    /// 
    /// MOTIVO: Permitir visualizar el detalle de cada producto vendido (cantidad, precio, descuentos, etc.)
    /// en una venta particular del sistema externo.
    /// 
    /// ESTRUCTURA DE PARÁMETROS:
    /// Estos parámetros identifican de forma única una venta en el sistema externo InforAVA.
    /// </summary>
       /// <returns>Lista de detalles de productos en esa venta, o lista vacía si no existen.</returns>
    Task<IEnumerable<ExternalSaleDetailDto>> GetSaleDetailAsync(string nf, string caja, string serie, string folio);
}
