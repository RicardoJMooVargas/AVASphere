using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Inventory.DTOs;
using AVASphere.ApplicationCore.Inventory.Entities.General;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using Microsoft.Extensions.Logging;

namespace AVASphere.Infrastructure.Inventory.Services;

public class LocationDetailsService : ILocationDetailsService
{
    private readonly ILocationDetailsRepository _locationDetailsRepository;
    private readonly IAreaRepository _areaRepository;
    private readonly ILogger<LocationDetailsService> _logger;

    public LocationDetailsService(
        ILocationDetailsRepository locationDetailsRepository,
        IAreaRepository areaRepository,
        ILogger<LocationDetailsService> logger)
    {
        _locationDetailsRepository = locationDetailsRepository;
        _areaRepository = areaRepository;
        _logger = logger;
    }

    public async Task<LocationDetailsResponseDto> CreateAsync(LocationDetailsRequestDto request, int? userAreaId = null)
    {
        try
        {
            _logger.LogInformation("Creando nueva LocationDetail");

            // Validar las entidades relacionadas existen
            int areaId = request.IdArea ?? userAreaId ?? throw new ArgumentException("Se requiere IdArea o debe estar autenticado con un área válida");

            var area = await _areaRepository.GetByIdAsync(areaId);
            if (area == null)
            {
                throw new KeyNotFoundException($"Área con ID {areaId} no encontrada");
            }

            // Verificar si ya existe una ubicación con los mismos parámetros
            var existingLocation = await _locationDetailsRepository.GetByLocationParametersAsync(
                areaId, 
                request.IdStorageStructure, 
                request.Section, 
                request.VerticalLevel);
                
            if (existingLocation != null)
            {
                throw new InvalidOperationException($"Ya existe una ubicación con los mismos parámetros: Área={areaId}, Estructura={request.IdStorageStructure}, Sección={request.Section}, Nivel={request.VerticalLevel}");
            }

            var locationDetails = new LocationDetails
            {
                TypeStorageSystem = request.TypeStorageSystem,
                Section = request.Section,
                VerticalLevel = request.VerticalLevel,
                IdArea = areaId,
                IdStorageStructure = request.IdStorageStructure
            };

            var created = await _locationDetailsRepository.CreateAsync(locationDetails);
            
            _logger.LogInformation("LocationDetail creada exitosamente con ID: {Id}", created.IdLocationDetails);
            return MapToResponseDto(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear LocationDetail");
            throw;
        }
    }

    public async Task<LocationDetailsResponseDto?> GetByIdAsync(int id)
    {
        try
        {
            var locationDetails = await _locationDetailsRepository.GetByIdAsync(id);
            return locationDetails != null ? MapToResponseDto(locationDetails) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener LocationDetail por ID: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<LocationDetailsResponseDto>> GetAllAsync()
    {
        try
        {
            var locationDetailsList = await _locationDetailsRepository.GetAllAsync();
            return locationDetailsList.Select(MapToResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todas las LocationDetails");
            throw;
        }
    }

    public async Task<IEnumerable<LocationDetailsResponseDto>> GetByAreaIdAsync(int idArea)
    {
        try
        {
            var locationDetailsList = await _locationDetailsRepository.GetByAreaIdAsync(idArea);
            return locationDetailsList.Select(MapToResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener LocationDetails por IdArea: {IdArea}", idArea);
            throw;
        }
    }

    public async Task<IEnumerable<LocationDetailsResponseDto>> GetByStorageStructureIdAsync(int idStorageStructure)
    {
        try
        {
            var locationDetailsList = await _locationDetailsRepository.GetByStorageStructureIdAsync(idStorageStructure);
            return locationDetailsList.Select(MapToResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener LocationDetails por IdStorageStructure: {IdStorageStructure}", idStorageStructure);
            throw;
        }
    }

    public async Task<LocationDetailsResponseDto?> GetByLocationParametersAsync(int idArea, int idStorageStructure, string section, int verticalLevel)
    {
        try
        {
            var locationDetails = await _locationDetailsRepository.GetByLocationParametersAsync(idArea, idStorageStructure, section, verticalLevel);
            return locationDetails != null ? MapToResponseDto(locationDetails) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener LocationDetail por parámetros");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            return await _locationDetailsRepository.ExistsAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar existencia de LocationDetail: {Id}", id);
            throw;
        }
    }

    public async Task<LocationDetailsResponseDto> UpdateAsync(int id, LocationDetailsUpdateDto request)
    {
        try
        {
            _logger.LogInformation("Actualizando LocationDetail con ID: {Id}", id);

            var existing = await _locationDetailsRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"LocationDetail con ID {id} no encontrada");
            }

            // Validar que el área existe
            var area = await _areaRepository.GetByIdAsync(request.IdArea);
            if (area == null)
            {
                throw new KeyNotFoundException($"Área con ID {request.IdArea} no encontrada");
            }

            // Verificar si ya existe otra ubicación con los mismos parámetros (excluyendo la actual)
            var existingLocation = await _locationDetailsRepository.GetByLocationParametersAsync(
                request.IdArea, 
                request.IdStorageStructure, 
                request.Section, 
                request.VerticalLevel);
                
            if (existingLocation != null && existingLocation.IdLocationDetails != id)
            {
                throw new InvalidOperationException($"Ya existe otra ubicación con los mismos parámetros: Área={request.IdArea}, Estructura={request.IdStorageStructure}, Sección={request.Section}, Nivel={request.VerticalLevel}");
            }

            existing.TypeStorageSystem = request.TypeStorageSystem;
            existing.Section = request.Section;
            existing.VerticalLevel = request.VerticalLevel;
            existing.IdArea = request.IdArea;
            existing.IdStorageStructure = request.IdStorageStructure;

            var updated = await _locationDetailsRepository.UpdateAsync(existing);
            
            _logger.LogInformation("LocationDetail actualizada exitosamente: {Id}", id);
            return MapToResponseDto(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar LocationDetail con ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            _logger.LogInformation("Eliminando LocationDetail con ID: {Id}", id);

            var existing = await _locationDetailsRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"LocationDetail con ID {id} no encontrada");
            }

            // La lógica de eliminar PhysicalInventoryDetail relacionados y ponerlos en null 
            // debería manejarse en el repository con las configuraciones de cascade
            var result = await _locationDetailsRepository.DeleteAsync(id);
            
            if (result)
            {
                _logger.LogInformation("LocationDetail eliminada exitosamente: {Id}", id);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar LocationDetail con ID: {Id}", id);
            throw;
        }
    }

    private LocationDetailsResponseDto MapToResponseDto(LocationDetails entity)
    {
        return new LocationDetailsResponseDto
        {
            IdLocationDetails = entity.IdLocationDetails,
            TypeStorageSystem = entity.TypeStorageSystem,
            Section = entity.Section,
            VerticalLevel = entity.VerticalLevel,
            IdArea = entity.IdArea,
            AreaName = entity.Area?.Name,
            AreaNormalizedName = entity.Area?.NormalizedName,
            IdStorageStructure = entity.IdStorageStructure,
            CodeRack = entity.StorageStructure?.CodeRack
        };
    }
}
