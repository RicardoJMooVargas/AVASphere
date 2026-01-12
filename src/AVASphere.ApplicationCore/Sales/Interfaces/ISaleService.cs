using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Sales.DTOs.ImportDTOs;
using AVASphere.ApplicationCore.Sales.Entities;

namespace AVASphere.ApplicationCore.Sales.Interfaces;

public interface ISaleService
{
    Task<Sale> CreateSaleAsync(SaleExternalDto saleDto, string createdByUserId, int customerId, string salesExecutive);
    Task<Sale?> GetSaleByIdAsync(int saleId);
    Task<Sale?> GetSaleByFolioAsync(string folio);
    Task<bool> DeleteSaleAsync(int id);

    // Operación de negocio: crear venta a partir de cotizaciones (transaccional)
    Task<Sale> CreateSaleFromQuotationsAsync(IEnumerable<int> quotationIds, Sale saleData, string createdByUserId);

    /// <summary>
    /// Crea una venta a partir de datos del sistema externo (InforAVA)
    /// y la vincula automáticamente con una cotización existente.
    /// 
    /// FLUJO TRANSACCIONAL:
    /// 1. Obtiene datos generales de la venta desde VENTASPORFECHAV (API externa)
    /// 2. Obtiene detalles de productos desde DetalleVentaV (API externa)
    /// 3. Registra la venta en la BD
    /// 4. Crea la relación SaleQuotation (vinculación N:N)
    /// 5. Opcionalmente marca la cotización como primaria
    /// 
    /// VALIDACIONES:
    /// - La cotización debe existir
    /// - El cliente (si se proporciona) debe existir
    /// - Los datos externos deben ser consistentes
    /// 
    /// DATOS ENRIQUECIDOS:
    /// - Se incluyen los productos en ProductsJson
    /// - Se guardan los datos auxiliares en AuxNoteDataJson
    /// - Se establece el folio externo para trazabilidad
    /// </summary>
    /// <param name="request">Parámetros para insertar la venta y vincular cotización</param>
    /// <param name="createdByUserId">Usuario que realiza la operación</param>
    /// <returns>La venta creada con su ID asignado</returns>
    Task<Sale> InsertExternalSaleAndLinkQuotationAsync(
        InsertExternalSaleAndQuotationRequest request,
        string createdByUserId
    );

    /// <summary>
    /// Importa ventas del sistema externo para un mes completo de forma optimizada.
    /// 
    /// OPTIMIZACIONES IMPLEMENTADAS:
    /// 1. Procesamiento por lotes para reducir carga en API externa
    /// 2. Cache de clientes existentes para evitar consultas repetitivas
    /// 3. Detección de duplicados antes de inserción
    /// 4. Limitación de concurrencia para no saturar API externa
    /// 5. Reintentos con backoff exponencial en caso de errores temporales
    /// 
    /// PROCESO:
    /// 1. Valida que el rango no exceda 31 días
    /// 2. Divide el mes en lotes de días (ej: 5 días por lote)
    /// 3. Para cada lote, consulta API externa
    /// 4. Verifica duplicados contra BD local
    /// 5. Crea clientes faltantes en lote
    /// 6. Obtiene detalles de productos para cada venta
    /// 7. Inserta ventas en lotes transaccionales
    /// 
    /// LIMITACIONES:
    /// - Máximo 1 mes (31 días)
    /// - Máximo 1000 ventas por importación
    /// - Timeout de 10 minutos por importación completa
    /// </summary>
    /// <param name="year">Año a importar</param>
    /// <param name="month">Mes a importar (1-12)</param>
    /// <param name="idConfigSys">ID del sistema de configuración</param>
    /// <param name="createdByUserId">Usuario que ejecuta la importación</param>
    /// <param name="batchSize">Tamaño del lote (por defecto 5 días)</param>
    /// <returns>Resultado de la importación con estadísticas</returns>
    Task<ImportSalesResult> ImportSalesForMonthAsync(
        int year, 
        int month, 
        int idConfigSys, 
        string createdByUserId, 
        int batchSize = 5
    );
}