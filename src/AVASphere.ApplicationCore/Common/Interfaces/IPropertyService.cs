using AVASphere.ApplicationCore.Common.DTOs;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IPropertyService
{
    // Create
    Task<PropertyResponseDto> CreateAsync(PropertyRequestDto propertyRequest);
    
    // Read
    Task<PropertyResponseDto?> GetByIdAsync(int id);
    Task<PropertyResponseDto?> GetByNameAsync(string name);
    Task<IEnumerable<PropertyResponseDto>> GetAllAsync();
    
    // Update
    Task<PropertyResponseDto> UpdateAsync(int id, PropertyRequestDto propertyRequest);
    
    // Delete
    Task<bool> DeleteAsync(int id);
}