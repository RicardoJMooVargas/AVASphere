using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AVASphere.Infrastructure.Common.Services;

public class AreaService : IAreaService
{
    private readonly IAreaRepository _areaRepository;
    private readonly ILogger<AreaService> _logger;

    public AreaService(IAreaRepository areaRepository, ILogger<AreaService> logger)
    {
        _areaRepository = areaRepository;
        _logger = logger;
    }

    public async Task<AreaResponseDto> CreateAsync(AreaRequestDto areaRequest)
    {
        try
        {
            // Validar si ya existe un área con el mismo nombre
            var existingArea = await _areaRepository.GetByNameAsync(areaRequest.Name);
            if (existingArea != null)
            {
                throw new InvalidOperationException($"Ya existe un área con el nombre: {areaRequest.Name}");
            }

            var area = new Area
            {
                Name = areaRequest.Name,
                NormalizedName = areaRequest.NormalizedName ?? areaRequest.Name.ToUpper()
            };

            var createdArea = await _areaRepository.CreateAsync(area);
            
            return new AreaResponseDto
            {
                IdArea = createdArea.IdArea,
                Name = createdArea.Name,
                NormalizedName = createdArea.NormalizedName,
                RolCount = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear área: {AreaName}", areaRequest.Name);
            throw;
        }
    }

    public async Task<AreaResponseDto?> GetByIdAsync(int id)
    {
        try
        {
            var area = await _areaRepository.GetByIdAsync(id);
            if (area == null) return null;

            return new AreaResponseDto
            {
                IdArea = area.IdArea,
                Name = area.Name,
                NormalizedName = area.NormalizedName,
                RolCount = area.Rol?.Count ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener área por ID: {AreaId}", id);
            throw;
        }
    }

    public async Task<AreaResponseDto?> GetByNameAsync(string name)
    {
        try
        {
            var area = await _areaRepository.GetByNameAsync(name);
            if (area == null) return null;

            return new AreaResponseDto
            {
                IdArea = area.IdArea,
                Name = area.Name,
                NormalizedName = area.NormalizedName,
                RolCount = area.Rol?.Count ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener área por nombre: {AreaName}", name);
            throw;
        }
    }

    public async Task<IEnumerable<AreaResponseDto>> GetAllAsync()
    {
        try
        {
            var areas = await _areaRepository.GetAllAsync();
            
            return areas.Select(area => new AreaResponseDto
            {
                IdArea = area.IdArea,
                Name = area.Name,
                NormalizedName = area.NormalizedName,
                RolCount = area.Rol?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todas las áreas");
            throw;
        }
    }

    public async Task<AreaResponseDto> UpdateAsync(int id, AreaRequestDto areaRequest)
    {
        try
        {
            var existingArea = await _areaRepository.GetByIdAsync(id);
            if (existingArea == null)
            {
                throw new KeyNotFoundException($"Área con ID {id} no encontrada");
            }

            // Validar si el nuevo nombre ya existe en otra área
            var areaWithSameName = await _areaRepository.GetByNameAsync(areaRequest.Name);
            if (areaWithSameName != null && areaWithSameName.IdArea != id)
            {
                throw new InvalidOperationException($"Ya existe otra área con el nombre: {areaRequest.Name}");
            }

            existingArea.Name = areaRequest.Name;
            existingArea.NormalizedName = areaRequest.NormalizedName ?? areaRequest.Name.ToUpper();

            var updatedArea = await _areaRepository.UpdateAsync(existingArea);
            
            return new AreaResponseDto
            {
                IdArea = updatedArea.IdArea,
                Name = updatedArea.Name,
                NormalizedName = updatedArea.NormalizedName,
                RolCount = updatedArea.Rol?.Count ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar área: {AreaId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            // Verificar si el área tiene roles asociados
            var roles = await _areaRepository.GetRolesByAreaAsync(id);
            if (roles.Any())
            {
                throw new InvalidOperationException("No se puede eliminar el área porque tiene roles asociados");
            }

            return await _areaRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar área: {AreaId}", id);
            throw;
        }
    }
}