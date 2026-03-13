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
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IPhysicalInventoryRepository _physicalInventoryRepository;
    private readonly IPhysicalInventoryDetailRepository _physicalInventoryDetailRepository;
    private readonly IStorageStructureRepository _storageStructureRepository;
    private readonly ILogger<LocationDetailsService> _logger;

    public LocationDetailsService(
        ILocationDetailsRepository locationDetailsRepository,
        IAreaRepository areaRepository,
        IInventoryRepository inventoryRepository,
        IPhysicalInventoryRepository physicalInventoryRepository,
        IPhysicalInventoryDetailRepository physicalInventoryDetailRepository,
        IStorageStructureRepository storageStructureRepository,
        ILogger<LocationDetailsService> logger)
    {
        _locationDetailsRepository = locationDetailsRepository;
        _areaRepository = areaRepository;
        _inventoryRepository = inventoryRepository;
        _physicalInventoryRepository = physicalInventoryRepository;
        _physicalInventoryDetailRepository = physicalInventoryDetailRepository;
        _storageStructureRepository = storageStructureRepository;
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
                
            LocationDetails created;
            if (existingLocation != null)
            {
                // Si ya existe, usar la ubicación existente
                created = existingLocation;
                _logger.LogInformation("Usando ubicación existente con ID: {Id}", created.IdLocationDetails);
            }
            else
            {
                // Si no existe, crear nueva ubicación
                var locationDetails = new LocationDetails
                {
                    Section = request.Section,
                    VerticalLevel = request.VerticalLevel,
                    IdArea = areaId,
                    IdStorageStructure = request.IdStorageStructure
                };

                created = await _locationDetailsRepository.CreateAsync(locationDetails);
                _logger.LogInformation("LocationDetail creada exitosamente con ID: {Id}", created.IdLocationDetails);
            }

            
            // Si se proporciona IdInventory, actualizar el registro de Inventory
            if (request.IdInventory.HasValue)
            {
                var inventory = await _inventoryRepository.GetByIdAsync(request.IdInventory.Value);
                if (inventory == null)
                {
                    throw new KeyNotFoundException($"Inventory con ID {request.IdInventory.Value} no encontrado");
                }
                inventory.LocationDetail = created.IdLocationDetails;
                await _inventoryRepository.UpdateAsync(inventory);
            }
            
            // Si se proporciona IdPhysicalInventoryDetail, actualizar el registro específico
            if (request.IdPhysicalInventoryDetail.HasValue)
            {
                var physicalInventoryDetail = await _physicalInventoryDetailRepository.GetByIdAsync(request.IdPhysicalInventoryDetail.Value);
                if (physicalInventoryDetail == null)
                {
                    throw new KeyNotFoundException($"PhysicalInventoryDetail con ID {request.IdPhysicalInventoryDetail.Value} no encontrado");
                }
                
                // Actualizar la ubicación del detalle de inventario físico
                physicalInventoryDetail.IdLocationDetails = created.IdLocationDetails;
                await _physicalInventoryDetailRepository.UpdateAsync(physicalInventoryDetail);
                _logger.LogInformation("PhysicalInventoryDetail actualizado con nueva ubicación. ID: {IdPhysicalInventoryDetail}, Nueva ubicación: {IdLocationDetails}", 
                    physicalInventoryDetail.IdPhysicalInventoryDetail, created.IdLocationDetails);
            }
            
            _logger.LogInformation("Operación completada para LocationDetail con ID: {Id}", created.IdLocationDetails);
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

            // Validar que el StorageStructure existe y pertenece al mismo Area
            var storageStructure = await _storageStructureRepository.GetByIdAsync(request.IdStorageStructure);
            if (storageStructure == null)
            {
                throw new KeyNotFoundException($"Estructura de almacenamiento con ID {request.IdStorageStructure} no encontrada");
            }
            
            if (storageStructure.IdArea.HasValue && storageStructure.IdArea.Value != request.IdArea)
            {
                throw new InvalidOperationException($"La estructura de almacenamiento {request.IdStorageStructure} pertenece al área {storageStructure.IdArea}, pero se está intentando actualizar para el área {request.IdArea}");
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

    /// <summary>
    /// Crea o obtiene una ubicación "SIN_ASIGNAR" para un área específica
    /// </summary>
    public async Task<LocationDetails> GetOrCreateDefaultLocationAsync(int idArea)
    {
        try
        {
            const string defaultSection = "SIN_ASIGNAR";
            const int defaultVerticalLevel = 0;

            // Buscar si ya existe una ubicación "SIN_ASIGNAR" en el área
            var existingDefault = await _locationDetailsRepository.GetByLocationParametersAsync(
                idArea, 
                0, // Usaremos 0 como indicador temporal
                defaultSection, 
                defaultVerticalLevel);

            if (existingDefault != null)
            {
                return existingDefault;
            }

            // Buscar una estructura de almacenamiento en el área o crear una genérica
            var storageStructures = await _storageStructureRepository.GetByAreaIdAsync(idArea);
            var defaultStorageStructureId = storageStructures?.FirstOrDefault()?.IdStorageStructure;

            if (!defaultStorageStructureId.HasValue)
            {
                _logger.LogWarning("No se encontró StorageStructure para área {IdArea}, creando ubicación sin estructura específica", idArea);
                // Si no hay estructura de almacenamiento, buscar cualquiera del sistema
                var anyStorageStructure = await _storageStructureRepository.GetAllAsync();
                defaultStorageStructureId = anyStorageStructure?.FirstOrDefault()?.IdStorageStructure;
                
                if (!defaultStorageStructureId.HasValue)
                {
                    throw new InvalidOperationException($"No hay estructuras de almacenamiento disponibles en el sistema para crear ubicación por defecto en área {idArea}");
                }
            }

            // Crear nueva ubicación "SIN_ASIGNAR"
            var defaultLocation = new LocationDetails
            {
                Section = defaultSection,
                VerticalLevel = defaultVerticalLevel,
                IdArea = idArea,
                IdStorageStructure = defaultStorageStructureId.Value
            };

            var created = await _locationDetailsRepository.CreateAsync(defaultLocation);
            _logger.LogInformation("Ubicación por defecto 'SIN_ASIGNAR' creada para área {IdArea} con ID {IdLocationDetails}", 
                idArea, created.IdLocationDetails);

            return created;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear ubicación por defecto para área {IdArea}", idArea);
            throw;
        }
    }

    private LocationDetailsResponseDto MapToResponseDto(LocationDetails entity)
    {
        return new LocationDetailsResponseDto
        {
            IdLocationDetails = entity.IdLocationDetails,
            Section = entity.Section,
            VerticalLevel = entity.VerticalLevel,
            IdArea = entity.IdArea,
            AreaName = entity.Area?.Name,
            AreaNormalizedName = entity.Area?.NormalizedName,
            IdStorageStructure = entity.IdStorageStructure,
            CodeRack = entity.StorageStructure?.CodeRack,
            TypeStorageSystem = entity.StorageStructure?.TypeStorageSystem
        };
    }
}
