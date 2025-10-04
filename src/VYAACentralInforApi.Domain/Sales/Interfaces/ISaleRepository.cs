using VYAACentralInforApi.Domain.Sales.Entities;

namespace VYAACentralInforApi.Domain.Sales.Interfaces;

public interface ISaleRepository
{
    Task<IEnumerable<Sale>> GetAllSalesAsync();
    Task<Sale?> GetSaleByIdAsync(string id);
    Task<Sale?> GetSaleByFolioAsync(string folio);
    Task<IEnumerable<Sale>> GetSalesByCustomerIdAsync(string customerId);
    Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Sale>> GetSalesBySalesExecutiveAsync(string salesExecutive);
    Task<Sale> CreateSaleAsync(Sale sale);
    Task<Sale> UpdateSaleAsync(Sale sale);
    Task<bool> DeleteSaleAsync(string id);
    Task<bool> SaleExistsAsync(string folio);
    Task<long> GetTotalSalesCountAsync();
    Task<decimal> GetTotalSalesAmountAsync();
    Task<decimal> GetTotalSalesAmountByDateRangeAsync(DateTime startDate, DateTime endDate);
}
