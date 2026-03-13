using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Inventory.DTOs;
using AVASphere.ApplicationCore.Inventory.Entities.General;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using Microsoft.Extensions.Logging;

namespace AVASphere.Infrastructure.Inventory.Services;

public class StorageStructureService : IStorageStructureService
{
    private readonly IStorageStructureRepository _storageStructureRepository;
    private readonly IAreaRepository _areaRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly ILogger<StorageStructureService> _logger;

    public StorageStructureService(
        IStorageStructureRepository storageStructureRepository,
        IAreaRepository areaRepository,
        IWarehouseRepository warehouseRepository,
        ILogger<StorageStructureService> logger)
    {
        _storageStructureRepository = storageStructureRepository;
        _areaRepository = areaRepository;
        _warehouseRepository = warehouseRepository;
        _logger = logger;
    }

    public async Task<StorageStructureResponseDto> CreateAsync(StorageStructureRequestDto storageStructureRequest)
    {
        try
        {
            // Validar si ya existe una estructura de almacenamiento con el mismo código
            var existingStorageStructure = await _storageStructureRepository.GetByCodeAsync(storageStructureRequest.CodeRack);
            if (existingStorageStructure != null)
            {
                throw new InvalidOperationException($"Ya existe una estructura de almacenamiento con el código: {storageStructureRequest.CodeRack}");
            }

            // Validar que el Warehouse existe
            var warehouse = await _warehouseRepository.GetByIdAsync(storageStructureRequest.IdWarehouse);
            if (warehouse == null)
            {
                throw new KeyNotFoundException($"El almacén con ID {storageStructureRequest.IdWarehouse} no existe");
            }

            // Validar que el Area existe si se proporciona
            if (storageStructureRequest.IdArea.HasValue)
            {
                var area = await _areaRepository.GetByIdAsync(storageStructureRequest.IdArea.Value);
                if (area == null)
                {
                    throw new KeyNotFoundException($"El área con ID {storageStructureRequest.IdArea.Value} no existe");
                }
            }

            var storageStructure = new StorageStructure
            {
                CodeRack = storageStructureRequest.CodeRack,
                TypeStorageSystem = storageStructureRequest.TypeStorageSystem,
                OneSection = storageStructureRequest.OneSection,
                HasLevel = storageStructureRequest.HasLevel,
                HasSubLevel = storageStructureRequest.HasSubLevel,
                IdWarehouse = storageStructureRequest.IdWarehouse,
                IdArea = storageStructureRequest.IdArea
            };

            var createdStorageStructure = await _storageStructureRepository.CreateAsync(storageStructure);
            
            return new StorageStructureResponseDto
            {
                IdStorageStructure = createdStorageStructure.IdStorageStructure,
                CodeRack = createdStorageStructure.CodeRack,
                TypeStorageSystem = createdStorageStructure.TypeStorageSystem,
                OneSection = createdStorageStructure.OneSection,
                HasLevel = createdStorageStructure.HasLevel,
                HasSubLevel = createdStorageStructure.HasSubLevel,
                IdWarehouse = createdStorageStructure.IdWarehouse,
                WarehouseName = createdStorageStructure.Warehouse?.Name ?? string.Empty,
                IdArea = createdStorageStructure.IdArea,
                AreaName = createdStorageStructure.Area?.Name,
                LocationDetailsCount = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear estructura de almacenamiento: {CodeRack}", storageStructureRequest.CodeRack);
            throw;
        }
    }

    public async Task<StorageStructureResponseDto?> GetByIdAsync(int id)
    {
        try
        {
            var storageStructure = await _storageStructureRepository.GetByIdAsync(id);
            if (storageStructure == null) return null;

            return new StorageStructureResponseDto
            {
                IdStorageStructure = storageStructure.IdStorageStructure,
                CodeRack = storageStructure.CodeRack,
                TypeStorageSystem = storageStructure.TypeStorageSystem,
                OneSection = storageStructure.OneSection,
                HasLevel = storageStructure.HasLevel,
                HasSubLevel = storageStructure.HasSubLevel,
                IdWarehouse = storageStructure.IdWarehouse,
                WarehouseName = storageStructure.Warehouse?.Name ?? string.Empty,
                IdArea = storageStructure.IdArea,
                AreaName = storageStructure.Area?.Name,
                LocationDetailsCount = storageStructure.LocationDetails?.Count ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estructura de almacenamiento por ID: {Id}", id);
            throw;
        }
    }

    public async Task<StorageStructureResponseDto?> GetByCodeRackAsync(string codeRack)
    {
        try
        {
            var storageStructure = await _storageStructureRepository.GetByCodeAsync(codeRack);
            if (storageStructure == null) return null;

            return new StorageStructureResponseDto
            {
                IdStorageStructure = storageStructure.IdStorageStructure,
                CodeRack = storageStructure.CodeRack,
                TypeStorageSystem = storageStructure.TypeStorageSystem,
                OneSection = storageStructure.OneSection,
                HasLevel = storageStructure.HasLevel,
                HasSubLevel = storageStructure.HasSubLevel,
                IdWarehouse = storageStructure.IdWarehouse,
                WarehouseName = storageStructure.Warehouse?.Name ?? string.Empty,
                IdArea = storageStructure.IdArea,
                AreaName = storageStructure.Area?.Name,
                LocationDetailsCount = storageStructure.LocationDetails?.Count ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estructura de almacenamiento por código: {CodeRack}", codeRack);
            throw;
        }
    }

    public async Task<IEnumerable<StorageStructureResponseDto>> GetAllAsync()
    {
        try
        {
            var storageStructures = await _storageStructureRepository.GetAllAsync();
            
            return storageStructures.Select(storageStructure => new StorageStructureResponseDto
            {
                IdStorageStructure = storageStructure.IdStorageStructure,
                CodeRack = storageStructure.CodeRack,
                TypeStorageSystem = storageStructure.TypeStorageSystem,
                OneSection = storageStructure.OneSection,
                HasLevel = storageStructure.HasLevel,
                HasSubLevel = storageStructure.HasSubLevel,
                IdWarehouse = storageStructure.IdWarehouse,
                WarehouseName = storageStructure.Warehouse?.Name ?? string.Empty,
                IdArea = storageStructure.IdArea,
                AreaName = storageStructure.Area?.Name,
                LocationDetailsCount = storageStructure.LocationDetails?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todas las estructuras de almacenamiento");
            throw;
        }
    }

    public async Task<IEnumerable<StorageStructureResponseDto>> GetByWarehouseIdAsync(int warehouseId)
    {
        try
        {
            var storageStructures = await _storageStructureRepository.GetByWarehouseAsync(warehouseId);
            
            return storageStructures.Select(storageStructure => new StorageStructureResponseDto
            {
                IdStorageStructure = storageStructure.IdStorageStructure,
                CodeRack = storageStructure.CodeRack,
                OneSection = storageStructure.OneSection,
                HasLevel = storageStructure.HasLevel,
                HasSubLevel = storageStructure.HasSubLevel,
                IdWarehouse = storageStructure.IdWarehouse,
                WarehouseName = storageStructure.Warehouse?.Name ?? string.Empty,
                IdArea = storageStructure.IdArea,
                AreaName = storageStructure.Area?.Name,
                LocationDetailsCount = storageStructure.LocationDetails?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estructuras de almacenamiento por almacén: {WarehouseId}", warehouseId);
            throw;
        }
    }

    public async Task<IEnumerable<StorageStructureResponseDto>> GetByAreaIdAsync(int areaId)
    {
        try
        {
            var allStorageStructures = await _storageStructureRepository.GetAllAsync();
            var storageStructures = allStorageStructures.Where(ss => ss.IdArea == areaId);
            
            return storageStructures.Select(storageStructure => new StorageStructureResponseDto
            {
                IdStorageStructure = storageStructure.IdStorageStructure,
                CodeRack = storageStructure.CodeRack,
                OneSection = storageStructure.OneSection,
                HasLevel = storageStructure.HasLevel,
                HasSubLevel = storageStructure.HasSubLevel,
                IdWarehouse = storageStructure.IdWarehouse,
                WarehouseName = storageStructure.Warehouse?.Name ?? string.Empty,
                IdArea = storageStructure.IdArea,
                AreaName = storageStructure.Area?.Name,
                LocationDetailsCount = storageStructure.LocationDetails?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estructuras de almacenamiento por área: {AreaId}", areaId);
            throw;
        }
    }

    public async Task<StorageStructureResponseDto> UpdateAsync(int id, StorageStructureRequestDto storageStructureRequest)
    {
        try
        {
            var existingStorageStructure = await _storageStructureRepository.GetByIdAsync(id);
            if (existingStorageStructure == null)
            {
                throw new KeyNotFoundException($"Estructura de almacenamiento con ID {id} no encontrada");
            }

            // Validar si el nuevo código ya existe en otra estructura
            var storageStructureWithSameCode = await _storageStructureRepository.GetByCodeAsync(storageStructureRequest.CodeRack);
            if (storageStructureWithSameCode != null && storageStructureWithSameCode.IdStorageStructure != id)
            {
                throw new InvalidOperationException($"Ya existe otra estructura de almacenamiento con el código: {storageStructureRequest.CodeRack}");
            }

            // Validar que el Warehouse existe
            var warehouse = await _warehouseRepository.GetByIdAsync(storageStructureRequest.IdWarehouse);
            if (warehouse == null)
            {
                throw new KeyNotFoundException($"El almacén con ID {storageStructureRequest.IdWarehouse} no existe");
            }

            // Validar que el Area existe si se proporciona
            if (storageStructureRequest.IdArea.HasValue)
            {
                var area = await _areaRepository.GetByIdAsync(storageStructureRequest.IdArea.Value);
                if (area == null)
                {
                    throw new KeyNotFoundException($"El área con ID {storageStructureRequest.IdArea.Value} no existe");
                }
            }

            existingStorageStructure.CodeRack = storageStructureRequest.CodeRack;
            existingStorageStructure.OneSection = storageStructureRequest.OneSection;
            existingStorageStructure.HasLevel = storageStructureRequest.HasLevel;
            existingStorageStructure.HasSubLevel = storageStructureRequest.HasSubLevel;
            existingStorageStructure.IdWarehouse = storageStructureRequest.IdWarehouse;
            existingStorageStructure.IdArea = storageStructureRequest.IdArea;

            var updatedStorageStructure = await _storageStructureRepository.UpdateAsync(existingStorageStructure);
            
            return new StorageStructureResponseDto
            {
                IdStorageStructure = updatedStorageStructure.IdStorageStructure,
                CodeRack = updatedStorageStructure.CodeRack,
                OneSection = updatedStorageStructure.OneSection,
                HasLevel = updatedStorageStructure.HasLevel,
                HasSubLevel = updatedStorageStructure.HasSubLevel,
                IdWarehouse = updatedStorageStructure.IdWarehouse,
                WarehouseName = updatedStorageStructure.Warehouse?.Name ?? string.Empty,
                IdArea = updatedStorageStructure.IdArea,
                AreaName = updatedStorageStructure.Area?.Name,
                LocationDetailsCount = updatedStorageStructure.LocationDetails?.Count ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar estructura de almacenamiento: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            // Verificar si la estructura de almacenamiento tiene detalles de ubicación asociados
            var storageStructure = await _storageStructureRepository.GetByIdAsync(id);
            if (storageStructure?.LocationDetails?.Any() == true)
            {
                throw new InvalidOperationException("No se puede eliminar la estructura de almacenamiento porque tiene detalles de ubicación asociados");
            }

            return await _storageStructureRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar estructura de almacenamiento: {Id}", id);
            throw;
        }
    }
}
