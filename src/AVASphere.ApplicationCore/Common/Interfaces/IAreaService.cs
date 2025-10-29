using AVASphere.ApplicationCore.Common.DTOs;

namespace AVASphere.ApplicationCore.Common.Interfaces;


public interface IAreaService
{
    // Create
    Task<AreaResponseDto> CreateAsync(AreaRequestDto areaRequest);
    
    // Read
    Task<AreaResponseDto?> GetByIdAsync(int id);
    Task<AreaResponseDto?> GetByNameAsync(string name);
    Task<IEnumerable<AreaResponseDto>> GetAllAsync();
    
    // Update
    Task<AreaResponseDto> UpdateAsync(int id, AreaRequestDto areaRequest);
    
    // Delete
    Task<bool> DeleteAsync(int id);
}