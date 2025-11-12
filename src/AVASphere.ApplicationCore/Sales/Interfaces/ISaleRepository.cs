using AVASphere.ApplicationCore.Sales.Entities;

namespace AVASphere.ApplicationCore.Sales.Interfaces;

public interface ISaleRepository
{
    Task<IEnumerable<Sale>> GetAllSalesAsync();
    Task<Sale?> GetSaleByIdAsync(int id);
    Task<Sale?> GetSaleByFolioAsync(string folio);
    Task<IEnumerable<Sale>> GetSalesByCustomerIdAsync(int customerId);
    Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Sale>> GetSalesBySalesExecutiveAsync(string salesExecutive);

    Task<Sale> CreateSaleAsync(Sale sale);
    Task<Sale> UpdateSaleAsync(Sale sale);
    Task<bool> DeleteSaleAsync(int id);
    
    Task<bool> SaleExistsAsync(string folio);
    Task<long> GetTotalSalesCountAsync();
    Task<decimal> GetTotalSalesAmountAsync();
    Task<decimal> GetTotalSalesAmountByDateRangeAsync(DateTime startDate, DateTime endDate);
}
