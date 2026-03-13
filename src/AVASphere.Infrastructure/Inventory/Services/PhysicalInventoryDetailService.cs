using AVASphere.ApplicationCore.Inventory.DTOs;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using AVASphere.ApplicationCore.Inventory.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AVASphere.Infrastructure.Inventory.Services;

/// <summary>
/// Implementación del servicio para manejar detalles de inventario físico
/// </summary>
public class PhysicalInventoryDetailService : IPhysicalInventoryDetailService
{
    private readonly MasterDbContext _context;
    private readonly ILogger<PhysicalInventoryDetailService> _logger;

    public PhysicalInventoryDetailService(MasterDbContext context, ILogger<PhysicalInventoryDetailService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PhysicalInventoryDetailResponseDto?> UpdatePhysicalQuantityAsync(PhysicalQuantityUpdateDto updateDto)
    {
        try
        {
            _logger.LogInformation("Actualizando cantidad física para detalle ID: {Id}", updateDto.IdPhysicalInventoryDetail);

            var detail = await _context.PhysicalInventoryDetails
                .Include(d => d.PhysicalInventory)
                .Include(d => d.Product)
                .Include(d => d.LocationDetails)
                .FirstOrDefaultAsync(d => d.IdPhysicalInventoryDetail == updateDto.IdPhysicalInventoryDetail);

            if (detail == null)
            {
                _logger.LogWarning("Detalle de inventario físico no encontrado con ID: {Id}", updateDto.IdPhysicalInventoryDetail);
                return null;
            }

            // Actualizar cantidad física y calcular diferencia
            detail.PhysicalQuantity = updateDto.PhysicalQuantity;
            detail.Difference = detail.PhysicalQuantity - detail.SystemQuantity;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cantidad física actualizada exitosamente. Nueva cantidad: {Quantity}, Diferencia: {Difference}", 
                detail.PhysicalQuantity, detail.Difference);

            return MapToResponseDto(detail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cantidad física para detalle ID: {Id}", updateDto.IdPhysicalInventoryDetail);
            throw;
        }
    }

    public async Task<PhysicalInventoryDetailResponseDto> CreatePhysicalInventoryDetailAsync(PhysicalInventoryDetailCreateDto createDto)
    {
        try
        {
            _logger.LogInformation("Creando nuevo detalle de inventario físico para producto ID: {ProductId}", createDto.IdProduct);

            // Verificar que el inventario físico existe
            var inventoryExists = await _context.PhysicalInventories
                .AnyAsync(pi => pi.IdPhysicalInventory == createDto.IdPhysicalInventory);

            if (!inventoryExists)
            {
                throw new ArgumentException($"El inventario físico con ID {createDto.IdPhysicalInventory} no existe");
            }

            // Verificar que el producto existe
            var productExists = await _context.Products
                .AnyAsync(p => p.IdProduct == createDto.IdProduct);

            if (!productExists)
            {
                throw new ArgumentException($"El producto con ID {createDto.IdProduct} no existe");
            }

            // Crear el nuevo detalle
            var detail = new PhysicalInventoryDetail
            {
                IdPhysicalInventory = createDto.IdPhysicalInventory,
                IdProduct = createDto.IdProduct,
                SystemQuantity = createDto.SystemQuantity,
                PhysicalQuantity = createDto.PhysicalQuantity,
                Difference = createDto.PhysicalQuantity - createDto.SystemQuantity,
                IdLocationDetails = createDto.IdLocationDetails
            };

            _context.PhysicalInventoryDetails.Add(detail);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Detalle de inventario físico creado exitosamente con ID: {Id}", detail.IdPhysicalInventoryDetail);

            // Cargar relaciones para la respuesta
            await _context.Entry(detail)
                .Reference(d => d.PhysicalInventory)
                .LoadAsync();
            
            await _context.Entry(detail)
                .Reference(d => d.Product)
                .LoadAsync();

            if (detail.IdLocationDetails.HasValue)
            {
                await _context.Entry(detail)
                    .Reference(d => d.LocationDetails)
                    .LoadAsync();
            }

            return MapToResponseDto(detail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear detalle de inventario físico para producto ID: {ProductId}", createDto.IdProduct);
            throw;
        }
    }

    public async Task<PhysicalInventoryDetailResponseDto?> GetPhysicalInventoryDetailByIdAsync(int id)
    {
        try
        {
            var detail = await _context.PhysicalInventoryDetails
                .Include(d => d.PhysicalInventory)
                .Include(d => d.Product)
                .Include(d => d.LocationDetails)
                .FirstOrDefaultAsync(d => d.IdPhysicalInventoryDetail == id);

            return detail != null ? MapToResponseDto(detail) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de inventario físico con ID: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<PhysicalInventoryDetailResponseDto>> GetPhysicalInventoryDetailsByInventoryIdAsync(int idPhysicalInventory)
    {
        try
        {
            var details = await _context.PhysicalInventoryDetails
                .Include(d => d.PhysicalInventory)
                .Include(d => d.Product)
                .Include(d => d.LocationDetails)
                .Where(d => d.IdPhysicalInventory == idPhysicalInventory)
                .ToListAsync();

            return details.Select(MapToResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalles de inventario físico para inventario ID: {InventoryId}", idPhysicalInventory);
            throw;
        }
    }

    public async Task<bool> RecalculateDifferencesAsync(int idPhysicalInventory)
    {
        try
        {
            _logger.LogInformation("Recalculando diferencias para inventario físico ID: {InventoryId}", idPhysicalInventory);

            var details = await _context.PhysicalInventoryDetails
                .Where(d => d.IdPhysicalInventory == idPhysicalInventory)
                .ToListAsync();

            if (!details.Any())
            {
                _logger.LogWarning("No se encontraron detalles para el inventario físico ID: {InventoryId}", idPhysicalInventory);
                return false;
            }

            foreach (var detail in details)
            {
                detail.Difference = detail.PhysicalQuantity - detail.SystemQuantity;
            }

            var affectedRows = await _context.SaveChangesAsync();
            _logger.LogInformation("Se recalcularon las diferencias para {Count} detalles", affectedRows);

            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al recalcular diferencias para inventario físico ID: {InventoryId}", idPhysicalInventory);
            throw;
        }
    }

    private static PhysicalInventoryDetailResponseDto MapToResponseDto(PhysicalInventoryDetail detail)
    {
        return new PhysicalInventoryDetailResponseDto
        {
            IdPhysicalInventoryDetail = detail.IdPhysicalInventoryDetail,
            SystemQuantity = detail.SystemQuantity,
            PhysicalQuantity = detail.PhysicalQuantity,
            Difference = detail.Difference,
            IdPhysicalInventory = detail.IdPhysicalInventory,
            PhysicalInventoryName = $"Inventario #{detail.IdPhysicalInventory}",
            IdProduct = detail.IdProduct,
            ProductName = detail.Product?.MainName ?? "N/A",
            ProductCode = detail.Product?.Unit ?? "N/A", // Usar Unit como código por ahora
            IdLocationDetails = detail.IdLocationDetails,
            LocationName = detail.LocationDetails != null ? 
                $"{detail.LocationDetails.Section} - Nivel {detail.LocationDetails.VerticalLevel}" : null,
            CreatedAt = detail.PhysicalInventory?.InventoryDate ?? DateTime.UtcNow,
            UpdatedAt = null // PhysicalInventory no tiene UpdatedAt
        };
    }
}


