using AVASphere.ApplicationCore.Inventory.DTOs;
using AVASphere.ApplicationCore.Inventory.Entities.General;

namespace AVASphere.ApplicationCore.Inventory.Interfaces;

public interface ILocationDetailsService
{
    // Create
    Task<LocationDetailsResponseDto> CreateAsync(LocationDetailsRequestDto request, int? userAreaId = null);
    
    // Read
    Task<LocationDetailsResponseDto?> GetByIdAsync(int id);
    Task<IEnumerable<LocationDetailsResponseDto>> GetAllAsync();
    Task<IEnumerable<LocationDetailsResponseDto>> GetByAreaIdAsync(int idArea);
    Task<IEnumerable<LocationDetailsResponseDto>> GetByStorageStructureIdAsync(int idStorageStructure);
    Task<LocationDetailsResponseDto?> GetByLocationParametersAsync(int idArea, int idStorageStructure, string section, int verticalLevel);
    Task<bool> ExistsAsync(int id);
    
    // Update
    Task<LocationDetailsResponseDto> UpdateAsync(int id, LocationDetailsUpdateDto request);
    
    // Delete
    Task<bool> DeleteAsync(int id);
}
