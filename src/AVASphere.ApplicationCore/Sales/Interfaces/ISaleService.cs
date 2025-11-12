using AVASphere.ApplicationCore.Sales.Entities;

namespace AVASphere.ApplicationCore.Sales.Interfaces;

public interface ISaleService
{
    Task<Sale> CreateSaleAsync(Sale sale, string createdByUserId);
    Task<Sale?> GetSaleByIdAsync(int saleId);
    Task<Sale?> GetSaleByFolioAsync(string folio);
    Task<bool> DeleteSaleAsync(int id);

    // Operación de negocio: crear venta a partir de cotizaciones (transaccional)
    Task<Sale> CreateSaleFromQuotationsAsync(IEnumerable<int> quotationIds, Sale saleData, string createdByUserId);
}