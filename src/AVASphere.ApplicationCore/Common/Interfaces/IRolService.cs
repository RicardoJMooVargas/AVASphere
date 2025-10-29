using AVASphere.ApplicationCore.Common.DTOs;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IRolService
{
    // Create
    Task<RolResponseDto> CreateAsync(RolRequestDto rolRequest);
    
    // Read
    Task<RolResponseDto?> GetByIdAsync(int id);
    Task<RolResponseDto?> GetByNameAsync(string name);
    Task<IEnumerable<RolResponseDto>> GetAllAsync();
    
    // Update
    Task<RolResponseDto> UpdateAsync(int id, RolRequestDto rolRequest);
    
    // Delete
    Task<bool> DeleteAsync(int id);
}