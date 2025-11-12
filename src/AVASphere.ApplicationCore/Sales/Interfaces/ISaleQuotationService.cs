using AVASphere.ApplicationCore.Sales.DTOs;
using AVASphere.ApplicationCore.Sales.Entities;

namespace AVASphere.ApplicationCore.Sales.Interfaces
{
    public interface ISaleQuotationService
    {
        // Operaciones públicas de negocio: lectura y desvinculado.
        Task<IEnumerable<SaleQuotation>> GetQuotationsForSaleAsync(int saleId);
        Task<SaleQuotation?> GetByIdAsync(int id);

        /// <summary>
        /// Desvincula (borrar) una cotización de una venta. La vinculación (Create) se hace desde el flujo de SaleService al crear la venta.
        /// </summary>
        Task<bool> UnlinkQuotationFromSaleAsync(int saleId, int quotationId);

        /// <summary>
        /// Marca una cotización como primaria para la venta (operación de negocio sobre links).
        /// </summary>
        Task<bool> MarkPrimaryQuotationAsync(int saleId, int quotationId, string requestedByUserId);
    }
}