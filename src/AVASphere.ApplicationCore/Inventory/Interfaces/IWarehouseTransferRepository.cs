using AVASphere.ApplicationCore.Inventory.Entities.General;

namespace AVASphere.ApplicationCore.Inventory.Interfaces;

public interface IWarehouseTransferRepository
{
    // Create
    Task<WarehouseTransfer> CreateAsync(WarehouseTransfer warehouseTransfer);
    
    // Read
    Task<WarehouseTransfer?> GetByIdAsync(int idWarehouseTransfer);
    Task<WarehouseTransfer?> GetByIdWithDetailsAsync(int idWarehouseTransfer);
    Task<IEnumerable<WarehouseTransfer>> GetAllAsync();
    Task<IEnumerable<WarehouseTransfer>> GetAllWithDetailsAsync();
    Task<IEnumerable<WarehouseTransfer>> GetByStatusAsync(string status);
    Task<IEnumerable<WarehouseTransfer>> GetByWarehouseFromAsync(int idWarehouseFrom);
    Task<IEnumerable<WarehouseTransfer>> GetByWarehouseToAsync(int idWarehouseTo);
    Task<IEnumerable<WarehouseTransfer>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> ExistsAsync(int idWarehouseTransfer);
    
    // Update
    Task<WarehouseTransfer> UpdateAsync(WarehouseTransfer warehouseTransfer);
    Task<bool> UpdateStatusAsync(int idWarehouseTransfer, string status);
    
    // Delete
    Task<bool> DeleteAsync(int idWarehouseTransfer);
}

