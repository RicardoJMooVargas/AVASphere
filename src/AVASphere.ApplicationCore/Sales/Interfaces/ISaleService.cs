using AVASphere.ApplicationCore.Sales.DTOs;
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
}