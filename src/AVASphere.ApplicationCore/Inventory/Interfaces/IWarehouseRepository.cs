using AVASphere.ApplicationCore.Inventory.Entities.General;

namespace AVASphere.ApplicationCore.Inventory.Interfaces;

public interface IWarehouseRepository
{
    // Create
    Task<Warehouse> CreateAsync(Warehouse warehouse);
    
    // Read
    Task<Warehouse?> GetByIdAsync(int idWarehouse);
    Task<Warehouse?> GetByCodeAsync(string code);
    Task<IEnumerable<Warehouse>> GetAllAsync();
    Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync();
    Task<Warehouse?> GetMainWarehouseAsync();
    Task<bool> ExistsAsync(int idWarehouse);
    Task<bool> ExistsByCodeAsync(string code);
    
    // Update
    Task<Warehouse> UpdateAsync(Warehouse warehouse);
    
    // Delete
    Task<bool> DeleteAsync(int idWarehouse);
}
