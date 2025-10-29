using AVASphere.ApplicationCore.Common.DTOs;
using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AVASphere.Infrastructure.Common.Services; 

public class RolService : IRolService
{
    private readonly IRolRepository _rolRepository;
    private readonly IAreaRepository _areaRepository;
    private readonly ILogger<RolService> _logger;

    public RolService(IRolRepository rolRepository, IAreaRepository areaRepository, ILogger<RolService> logger)
    {
        _rolRepository = rolRepository;
        _areaRepository = areaRepository;
        _logger = logger;
    }

    public async Task<RolResponseDto> CreateAsync(RolRequestDto rolRequest)
    {
        try
        {
            // Validar si el área existe
            var area = await _areaRepository.GetByIdAsync(rolRequest.IdArea);
            if (area == null)
            {
                throw new KeyNotFoundException($"Área con ID {rolRequest.IdArea} no encontrada");
            }

            // Validar si ya existe un rol con el mismo nombre
            var existingRol = await _rolRepository.GetByNameAsync(rolRequest.Name);
            if (existingRol != null)
            {
                throw new InvalidOperationException($"Ya existe un rol con el nombre: {rolRequest.Name}");
            }

            var rol = new Rol
            {
                Name = rolRequest.Name,
                NormalizedName = rolRequest.NormalizedName ?? rolRequest.Name.ToUpper(),
                IdArea = rolRequest.IdArea
            };

            var createdRol = await _rolRepository.CreateAsync(rol);
            
            return new RolResponseDto
            {
                IdRol = createdRol.IdRol,
                Name = createdRol.Name,
                NormalizedName = createdRol.NormalizedName,
                IdArea = createdRol.IdArea,
                AreaName = area.Name,
                UserCount = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear rol: {RolName}", rolRequest.Name);
            throw;
        }
    }

    public async Task<RolResponseDto?> GetByIdAsync(int id)
    {
        try
        {
            var rol = await _rolRepository.GetByIdAsync(id);
            if (rol == null) return null;

            return new RolResponseDto
            {
                IdRol = rol.IdRol,
                Name = rol.Name,
                NormalizedName = rol.NormalizedName,
                IdArea = rol.IdArea,
                AreaName = rol.Area?.Name ?? "N/A",
                UserCount = rol.User?.Count ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener rol por ID: {RolId}", id);
            throw;
        }
    }

    public async Task<RolResponseDto?> GetByNameAsync(string name)
    {
        try
        {
            var rol = await _rolRepository.GetByNameAsync(name);
            if (rol == null) return null;

            return new RolResponseDto
            {
                IdRol = rol.IdRol,
                Name = rol.Name,
                NormalizedName = rol.NormalizedName,
                IdArea = rol.IdArea,
                AreaName = rol.Area?.Name ?? "N/A",
                UserCount = rol.User?.Count ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener rol por nombre: {RolName}", name);
            throw;
        }
    }

    public async Task<IEnumerable<RolResponseDto>> GetAllAsync()
    {
        try
        {
            var roles = await _rolRepository.GetAllAsync();
            
            return roles.Select(rol => new RolResponseDto
            {
                IdRol = rol.IdRol,
                Name = rol.Name,
                NormalizedName = rol.NormalizedName,
                IdArea = rol.IdArea,
                AreaName = rol.Area?.Name ?? "N/A",
                UserCount = rol.User?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los roles");
            throw;
        }
    }

    public async Task<RolResponseDto> UpdateAsync(int id, RolRequestDto rolRequest)
    {
        try
        {
            var existingRol = await _rolRepository.GetByIdAsync(id);
            if (existingRol == null)
            {
                throw new KeyNotFoundException($"Rol con ID {id} no encontrado");
            }

            // Validar si el área existe
            var area = await _areaRepository.GetByIdAsync(rolRequest.IdArea);
            if (area == null)
            {
                throw new KeyNotFoundException($"Área con ID {rolRequest.IdArea} no encontrada");
            }

            // Validar si el nuevo nombre ya existe en otro rol
            var rolWithSameName = await _rolRepository.GetByNameAsync(rolRequest.Name);
            if (rolWithSameName != null && rolWithSameName.IdRol != id)
            {
                throw new InvalidOperationException($"Ya existe otro rol con el nombre: {rolRequest.Name}");
            }

            existingRol.Name = rolRequest.Name;
            existingRol.NormalizedName = rolRequest.NormalizedName ?? rolRequest.Name.ToUpper();
            existingRol.IdArea = rolRequest.IdArea;

            var updatedRol = await _rolRepository.UpdateAsync(existingRol);
            
            return new RolResponseDto
            {
                IdRol = updatedRol.IdRol,
                Name = updatedRol.Name,
                NormalizedName = updatedRol.NormalizedName,
                IdArea = updatedRol.IdArea,
                AreaName = area.Name,
                UserCount = updatedRol.User?.Count ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar rol: {RolId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            // Verificar si el rol tiene usuarios asociados
            var rol = await _rolRepository.GetByIdAsync(id);
            if (rol?.User?.Any() == true)
            {
                throw new InvalidOperationException("No se puede eliminar el rol porque tiene usuarios asociados");
            }

            return await _rolRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar rol: {RolId}", id);
            throw;
        }
    }
}