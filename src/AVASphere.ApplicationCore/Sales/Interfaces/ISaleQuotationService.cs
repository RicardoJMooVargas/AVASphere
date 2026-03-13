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

        /// <summary>
        /// Gestiona relaciones complejas entre ventas y cotizaciones.
        /// 
        /// OPERACIONES SOPORTADAS:
        /// 1. DELETE: Elimina solo la relación (venta permanece en BD)
        /// 2. DELETE_WITH_SALE: Elimina la relación y la venta asociada (cascada)
        /// 3. REASSIGN: Reasigna la cotización de una venta a otra venta distinta
        /// 
        /// MOTIVO:
        /// - Manejar casos complejos de reconfiguración de relaciones
        /// - Soportar ventas importadas/externas que necesitan ajustes
        /// - Permitir transferencias de cotizaciones entre ventas
        /// - Limpiar datos incorrectamente vinculados
        /// 
        /// TRANSACCIONALIDAD:
        /// - Las operaciones DELETE_WITH_SALE son transaccionales (todo o nada)
        /// - Si falla la eliminación de la venta, se revierte toda la operación
        /// </summary>
        /// <param name="request">Parámetros de la operación a realizar</param>
        /// <param name="requestedByUserId">Usuario que solicita la operación (auditoría)</param>
        /// <returns>Respuesta con detalles de la operación realizada</returns>
        Task<ManageSaleQuotationRelationshipResponse> ManageRelationshipAsync(
            ManageSaleQuotationRelationshipRequest request,
            string requestedByUserId
        );
    }
}
    
