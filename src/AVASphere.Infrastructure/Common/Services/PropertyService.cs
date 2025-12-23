using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.ApplicationCore.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AVASphere.Infrastructure.Common.Services;

public class PropertyService : IPropertyService
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly ILogger<PropertyService> _logger;
    
    public PropertyService(IPropertyRepository propertyRepository, ILogger<PropertyService> logger)
    {
        _propertyRepository = propertyRepository;
        _logger = logger;
    }
    
    public async Task<PropertyResponseDto> CreateAsync(PropertyRequestDto propertyRequest)
    {
        try
        {
            // Validar si ya existe una propiedad con el mismo nombre
            var existingProperty = await _propertyRepository.GetByNameAsync(propertyRequest.Name);
            if (existingProperty != null)
            {
                throw new InvalidOperationException($"Ya existe una propiedad con el nombre: {propertyRequest.Name}");
            }
    
            var property = new Property
            {
                Name = propertyRequest.Name,
                NormalizedName = propertyRequest.NormalizedName ?? propertyRequest.Name.ToUpper()
            };
    
            var createdProperty = await _propertyRepository.CreateAsync(property);
                
            return new PropertyResponseDto
            {
                IdProperty = createdProperty.IdProperty,
                Name = createdProperty.Name!,
                NormalizedName = createdProperty.NormalizedName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la propiedad: {PropertyName}", propertyRequest.Name);
            throw;
        }
    }
    
    public async Task<PropertyResponseDto?> GetByIdAsync(int id)
    {
        try
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null) return null;

            return new PropertyResponseDto
            {
                IdProperty = property.IdProperty,
                Name = property.Name!,
                NormalizedName = property.NormalizedName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la propiedad por ID: {PropertyId}", id);
            throw;
        }
    }
    
    public async Task<IEnumerable<PropertyResponseDto>> GetAllAsync()
    {
        try
        {
            var properties = await _propertyRepository.GetAllAsync();
            
            return properties.Select(property => new PropertyResponseDto
            {
                IdProperty = property.IdProperty,
                Name = property.Name!,
                NormalizedName = property.NormalizedName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todas las propiedades");
            throw;
        }
    }
    
    public async Task<PropertyResponseDto> UpdateAsync(int id, PropertyRequestDto propertyRequest)
    {
        try
        {
            var existingProperty = await _propertyRepository.GetByIdAsync(id);
            if (existingProperty == null)
            {
                throw new KeyNotFoundException($"Propiedad con ID {id} no encontrada");
            }

            // Validar si el nuevo nombre ya existe en otra propiedad
            var propertyWithSameName = await _propertyRepository.GetByNameAsync(propertyRequest.Name);
            if (propertyWithSameName != null && propertyWithSameName.IdProperty != id)
            {
                throw new InvalidOperationException($"Ya existe otra propiedad con el nombre: {propertyRequest.Name}");
            }

            existingProperty.Name = propertyRequest.Name;
            existingProperty.NormalizedName = propertyRequest.NormalizedName ?? propertyRequest.Name.ToUpper();

            var updatedProperty = await _propertyRepository.UpdateAsync(existingProperty);
            
            return new PropertyResponseDto
            {
                IdProperty = updatedProperty.IdProperty,
                Name = updatedProperty.Name!,
                NormalizedName = updatedProperty.NormalizedName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la propiedad: {PropertyId}", id);
            throw;
        }
    }
    
    public async Task<PropertyResponseDto?> GetByNameAsync(string name)
    {
        try
        {
            var property = await _propertyRepository.GetByNameAsync(name);
            if (property == null) return null;

            return new PropertyResponseDto
            {
                IdProperty = property.IdProperty,
                Name = property.Name!,
                NormalizedName = property.NormalizedName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la propiedad por nombre: {PropertyName}", name);
            throw;
        }
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var existingProperty = await _propertyRepository.GetByIdAsync(id);
            if (existingProperty == null)
            {
                _logger.LogWarning("Intento de eliminar propiedad inexistente: {PropertyId}", id);
                return false;
            }

            return await _propertyRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la propiedad: {PropertyId}", id);
            throw;
        }
    }
    
    public async Task<bool> VerifyAsync(int id)
    {
        try
        {
            // Verificar si la propiedad tiene valores asociados
            var properties = await _propertyRepository.GetPropertyValuesByPropertyAsync(id);
            if (properties.Any())
            {
                throw new InvalidOperationException("No se puede eliminar la propiedad porque tiene valores asociados");
            }

            return await _propertyRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la propiedad: {PropertyId}", id);
            throw;
        }
    }
}
