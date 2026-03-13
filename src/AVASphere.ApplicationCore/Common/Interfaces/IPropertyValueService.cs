using AVASphere.ApplicationCore.Common.DTOs;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IPropertyValueService
{
    // Create
    Task<PropertyValueResponseDto> CreateAsync(PropertyValueRequestDto propertyValueRequest);
    
    // Read
    Task<PropertyValueResponseDto?> GetByIdAsync(int id);
    Task<PropertyValueResponseDto?> GetByValueAsync(string value);
    Task<IEnumerable<PropertyValueResponseDto>> GetAllAsync();
    Task<IEnumerable<PropertyValueResponseDto>> GetFilteredAsync(PropertyValueFilterDto filter);
    
    // Update
    Task<PropertyValueResponseDto> UpdateAsync(int id, PropertyValueUpdateDto propertyValueUpdate);
    
    // Delete
    Task<bool> DeleteAsync(int id);
    Task<bool> ForceDeleteAsync(int id);
}

