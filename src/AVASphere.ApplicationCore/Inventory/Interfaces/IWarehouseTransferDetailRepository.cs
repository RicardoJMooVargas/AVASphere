using AVASphere.ApplicationCore.Inventory.Entities.General;

namespace AVASphere.ApplicationCore.Inventory.Interfaces;

public interface IWarehouseTransferDetailRepository
{
    // Create
    Task<WarehouseTransferDetail> CreateAsync(WarehouseTransferDetail transferDetail);
    Task<IEnumerable<WarehouseTransferDetail>> CreateBatchAsync(IEnumerable<WarehouseTransferDetail> transferDetails);
    
    // Read
    Task<WarehouseTransferDetail?> GetByIdAsync(int idTransferDetail);
    Task<IEnumerable<WarehouseTransferDetail>> GetAllAsync();
    Task<IEnumerable<WarehouseTransferDetail>> GetByTransferIdAsync(int idWarehouseTransfer);
    Task<IEnumerable<WarehouseTransferDetail>> GetByProductIdAsync(int idProduct);
    Task<bool> ExistsAsync(int idTransferDetail);
    
    // Update
    Task<WarehouseTransferDetail> UpdateAsync(WarehouseTransferDetail transferDetail);
    
    // Delete
    Task<bool> DeleteAsync(int idTransferDetail);
    Task<bool> DeleteByTransferIdAsync(int idWarehouseTransfer);
}

