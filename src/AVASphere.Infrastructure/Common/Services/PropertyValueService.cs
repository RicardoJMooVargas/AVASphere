using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.ApplicationCore.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AVASphere.Infrastructure.Common.Services;

public class PropertyValueService : IPropertyValueService
{
    private readonly IPropertyValueRepository _propertyValueRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly ILogger<PropertyValueService> _logger;

    public PropertyValueService(
        IPropertyValueRepository propertyValueRepository,
        IPropertyRepository propertyRepository,
        ILogger<PropertyValueService> logger)
    {
        _propertyValueRepository = propertyValueRepository;
        _propertyRepository = propertyRepository;
        _logger = logger;
    }

    public async Task<PropertyValueResponseDto> CreateAsync(PropertyValueRequestDto propertyValueRequest)
    {
        try
        {
            // Validar que la propiedad existe
            var property = await _propertyRepository.GetByIdAsync(propertyValueRequest.IdProperty);
            if (property == null)
            {
                throw new KeyNotFoundException($"Property con ID {propertyValueRequest.IdProperty} no encontrada");
            }

            // Validar que no exista un valor duplicado para la misma propiedad
            var existingValues = await _propertyValueRepository.GetByPropertyAsync(propertyValueRequest.IdProperty);
            if (existingValues.Any(v => v.Value!.ToUpper() == propertyValueRequest.Value!.ToUpper()))
            {
                throw new InvalidOperationException($"Ya existe un PropertyValue con el valor '{propertyValueRequest.Value}' para la propiedad '{property.Name}'");
            }

            var propertyValue = new PropertyValue
            {
                Value = propertyValueRequest.Value,
                IdProperty = propertyValueRequest.IdProperty
            };

            var createdPropertyValue = await _propertyValueRepository.CreateAsync(propertyValue);
            
            // Recargar para obtener la propiedad relacionada
            var result = await _propertyValueRepository.GetByIdAsync(createdPropertyValue.IdPropertyValue);

            return new PropertyValueResponseDto
            {
                IdPropertyValue = result!.IdPropertyValue,
                Value = result.Value,
                NameProperty = result.Property.Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear PropertyValue");
            throw;
        }
    }

    public async Task<PropertyValueResponseDto?> GetByIdAsync(int id)
    {
        try
        {
            var propertyValue = await _propertyValueRepository.GetByIdAsync(id);
            if (propertyValue == null) return null;

            return new PropertyValueResponseDto
            {
                IdPropertyValue = propertyValue.IdPropertyValue,
                Value = propertyValue.Value,
                NameProperty = propertyValue.Property.Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener PropertyValue por ID: {PropertyValueId}", id);
            throw;
        }
    }

    public async Task<PropertyValueResponseDto?> GetByValueAsync(string value)
    {
        try
        {
            var propertyValue = await _propertyValueRepository.GetByValueAsync(value);
            if (propertyValue == null) return null;

            return new PropertyValueResponseDto
            {
                IdPropertyValue = propertyValue.IdPropertyValue,
                Value = propertyValue.Value,
                NameProperty = propertyValue.Property.Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener PropertyValue por valor: {Value}", value);
            throw;
        }
    }

    public async Task<IEnumerable<PropertyValueResponseDto>> GetAllAsync()
    {
        try
        {
            var propertyValues = await _propertyValueRepository.GetAllAsync();

            return propertyValues.Select(pv => new PropertyValueResponseDto
            {
                IdPropertyValue = pv.IdPropertyValue,
                Value = pv.Value,
                NameProperty = pv.Property.Name
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los PropertyValues");
            throw;
        }
    }

    public async Task<IEnumerable<PropertyValueResponseDto>> GetFilteredAsync(PropertyValueFilterDto filter)
    {
        try
        {
            IEnumerable<PropertyValue> propertyValues;

            // Filtro por IdPropertyValue específico
            if (filter.IdPropertyValue.HasValue)
            {
                var propertyValue = await _propertyValueRepository.GetByIdAsync(filter.IdPropertyValue.Value);
                propertyValues = propertyValue != null ? new[] { propertyValue } : Array.Empty<PropertyValue>();
            }
            // Filtro por IdPropertyOrName (numérico = ID, alfanumérico = nombre)
            else if (!string.IsNullOrEmpty(filter.IdPropertyOrName))
            {
                int propertyId;
                
                // Si es numérico, buscar por IdProperty
                if (int.TryParse(filter.IdPropertyOrName, out propertyId))
                {
                    propertyValues = await _propertyValueRepository.GetByPropertyAsync(propertyId);
                }
                // Si es alfanumérico, buscar primero la propiedad por nombre
                else
                {
                    var property = await _propertyRepository.GetByNameAsync(filter.IdPropertyOrName);
                    if (property == null)
                    {
                        return Array.Empty<PropertyValueResponseDto>();
                    }
                    propertyValues = await _propertyValueRepository.GetByPropertyAsync(property.IdProperty);
                }
            }
            // Filtro por Value con coincidencias normalizadas
            else if (!string.IsNullOrEmpty(filter.Value))
            {
                var allValues = await _propertyValueRepository.GetAllAsync();
                propertyValues = allValues.Where(pv => 
                    pv.Value!.ToUpper().Contains(filter.Value.ToUpper()));
            }
            // Sin filtros, devolver todos
            else
            {
                propertyValues = await _propertyValueRepository.GetAllAsync();
            }

            return propertyValues.Select(pv => new PropertyValueResponseDto
            {
                IdPropertyValue = pv.IdPropertyValue,
                Value = pv.Value,
                NameProperty = pv.Property.Name
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener PropertyValues filtrados");
            throw;
        }
    }

    public async Task<PropertyValueResponseDto> UpdateAsync(int id, PropertyValueUpdateDto propertyValueUpdate)
    {
        try
        {
            var existingPropertyValue = await _propertyValueRepository.GetByIdAsync(id);
            if (existingPropertyValue == null)
            {
                throw new KeyNotFoundException($"PropertyValue con ID {id} no encontrado");
            }

            // Actualizar Value si se proporciona
            if (!string.IsNullOrEmpty(propertyValueUpdate.Value))
            {
                // Verificar que no exista otro valor duplicado para la misma propiedad
                var existingValues = await _propertyValueRepository.GetByPropertyAsync(existingPropertyValue.IdProperty);
                if (existingValues.Any(v => v.IdPropertyValue != id && 
                                           v.Value!.ToUpper() == propertyValueUpdate.Value.ToUpper()))
                {
                    throw new InvalidOperationException($"Ya existe otro PropertyValue con el valor '{propertyValueUpdate.Value}' para esta propiedad");
                }
                existingPropertyValue.Value = propertyValueUpdate.Value;
            }

            // Actualizar IdProperty si se proporciona
            if (propertyValueUpdate.IdProperty.HasValue)
            {
                // Verificar que la nueva propiedad existe
                var newProperty = await _propertyRepository.GetByIdAsync(propertyValueUpdate.IdProperty.Value);
                if (newProperty == null)
                {
                    throw new KeyNotFoundException($"Property con ID {propertyValueUpdate.IdProperty.Value} no encontrada");
                }

                // Verificar que no exista un valor duplicado en la nueva propiedad
                var existingValuesInNewProperty = await _propertyValueRepository.GetByPropertyAsync(propertyValueUpdate.IdProperty.Value);
                if (existingValuesInNewProperty.Any(v => v.IdPropertyValue != id && 
                                                         v.Value!.ToUpper() == existingPropertyValue.Value!.ToUpper()))
                {
                    throw new InvalidOperationException($"Ya existe un PropertyValue con el valor '{existingPropertyValue.Value}' en la propiedad '{newProperty.Name}'");
                }

                existingPropertyValue.IdProperty = propertyValueUpdate.IdProperty.Value;
            }

            var updatedPropertyValue = await _propertyValueRepository.UpdateAsync(existingPropertyValue);
            
            // Recargar para obtener la propiedad relacionada actualizada
            var result = await _propertyValueRepository.GetByIdAsync(updatedPropertyValue.IdPropertyValue);

            return new PropertyValueResponseDto
            {
                IdPropertyValue = result!.IdPropertyValue,
                Value = result.Value,
                NameProperty = result.Property.Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar PropertyValue: {PropertyValueId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var existingPropertyValue = await _propertyValueRepository.GetByIdAsync(id);
            if (existingPropertyValue == null)
            {
                _logger.LogWarning("Intento de eliminar PropertyValue inexistente: {PropertyValueId}", id);
                return false;
            }

            // Verificar si está siendo usado en ProductProperties
            var productProperties = await _propertyValueRepository.GetProductPropertiesByPropertyValueAsync(id);
            if (productProperties.Any())
            {
                throw new InvalidOperationException("No se puede eliminar el PropertyValue porque está siendo usado en ProductProperties. Use el endpoint de eliminación forzada si desea continuar.");
            }

            // Verificar si está siendo usado en IndividualListingProperties
            var individualListingProperties = await _propertyValueRepository.GetIndividualListingPropertiesByPropertyValueAsync(id);
            if (individualListingProperties.Any())
            {
                throw new InvalidOperationException("No se puede eliminar el PropertyValue porque está siendo usado en IndividualListingProperties. Use el endpoint de eliminación forzada si desea continuar.");
            }

            return await _propertyValueRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar PropertyValue: {PropertyValueId}", id);
            throw;
        }
    }

    public async Task<bool> ForceDeleteAsync(int id)
    {
        try
        {
            var existingPropertyValue = await _propertyValueRepository.GetByIdAsync(id);
            if (existingPropertyValue == null)
            {
                _logger.LogWarning("Intento de eliminar PropertyValue inexistente: {PropertyValueId}", id);
                return false;
            }

            // Buscar o crear un valor genérico para la misma propiedad
            var genericValues = await _propertyValueRepository.GetByPropertyAsync(existingPropertyValue.IdProperty);
            var genericValue = genericValues.FirstOrDefault(v => v.Value!.ToUpper() == "GENERIC");

            // Si no existe, crear el valor genérico
            if (genericValue == null)
            {
                var newGenericValue = new PropertyValue
                {
                    Value = "Generic",
                    IdProperty = existingPropertyValue.IdProperty
                };
                genericValue = await _propertyValueRepository.CreateAsync(newGenericValue);
            }

            // Reemplazar todas las referencias en ProductProperties
            var productProperties = await _propertyValueRepository.GetProductPropertiesByPropertyValueAsync(id);
            foreach (var pp in productProperties)
            {
                pp.IdPropertyValue = genericValue.IdPropertyValue;
            }

            // Reemplazar todas las referencias en IndividualListingProperties
            var individualListingProperties = await _propertyValueRepository.GetIndividualListingPropertiesByPropertyValueAsync(id);
            foreach (var ilp in individualListingProperties)
            {
                ilp.IdPropertyValue = genericValue.IdPropertyValue;
            }

            // Ahora eliminar el PropertyValue original
            return await _propertyValueRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al forzar eliminación de PropertyValue: {PropertyValueId}", id);
            throw;
        }
    }
}

