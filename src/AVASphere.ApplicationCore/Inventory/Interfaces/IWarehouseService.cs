using AVASphere.ApplicationCore.Inventory.DTOs;

namespace AVASphere.ApplicationCore.Inventory.Interfaces;

public interface IWarehouseService
{
    Task<WarehouseResponseDto> CreateAsync(WarehouseRequestDto request);
    Task<IEnumerable<WarehouseResponseDto>> GetAllAsync();
    Task<WarehouseResponseDto?> GetByIdAsync(int id);
    Task<WarehouseResponseDto?> GetByCodeAsync(string code);
    Task<WarehouseResponseDto> UpdateAsync(int id, WarehouseRequestDto request);
    Task<bool> DeleteAsync(int id);
}