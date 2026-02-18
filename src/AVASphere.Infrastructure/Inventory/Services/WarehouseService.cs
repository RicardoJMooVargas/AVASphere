using AVASphere.ApplicationCore.Inventory.DTOs;
using AVASphere.ApplicationCore.Inventory.Entities.General;
using AVASphere.ApplicationCore.Inventory.Interfaces;

namespace AVASphere.Infrastructure.Inventory.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;

    public WarehouseService(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    public async Task<WarehouseResponseDto> CreateAsync(WarehouseRequestDto request)
    {
        ValidateRequest(request);

        var code = request.Code.Trim();
        var existsByCode = await _warehouseRepository.ExistsByCodeAsync(code);
        if (existsByCode)
            throw new InvalidOperationException($"Warehouse code '{code}' already exists.");

        var warehouse = new Warehouse
        {
            Name = request.Name.Trim(),
            Code = code,
            Location = request.Location?.Trim(),
            IsMain = request.IsMain,
            Active = request.Active
        };

        var createdWarehouse = await _warehouseRepository.CreateAsync(warehouse);
        return MapToResponse(createdWarehouse);
    }

    public async Task<IEnumerable<WarehouseResponseDto>> GetAllAsync()
    {
        var warehouses = await _warehouseRepository.GetAllAsync();
        return warehouses.Select(MapToResponse);
    }

    public async Task<WarehouseResponseDto?> GetByIdAsync(int id)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        return warehouse is null ? null : MapToResponse(warehouse);
    }

    public async Task<WarehouseResponseDto?> GetByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var warehouse = await _warehouseRepository.GetByCodeAsync(code.Trim());
        return warehouse is null ? null : MapToResponse(warehouse);
    }

    public async Task<WarehouseResponseDto> UpdateAsync(int id, WarehouseRequestDto request)
    {
        ValidateRequest(request);

        var existingWarehouse = await _warehouseRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Warehouse with ID {id} not found");

        var code = request.Code.Trim();
        var duplicatedWarehouse = await _warehouseRepository.GetByCodeAsync(code);
        if (duplicatedWarehouse is not null && duplicatedWarehouse.IdWarehouse != id)
            throw new InvalidOperationException($"Warehouse code '{code}' already exists.");

        existingWarehouse.Name = request.Name.Trim();
        existingWarehouse.Code = code;
        existingWarehouse.Location = request.Location?.Trim();
        existingWarehouse.IsMain = request.IsMain;
        existingWarehouse.Active = request.Active;

        var updatedWarehouse = await _warehouseRepository.UpdateAsync(existingWarehouse);
        return MapToResponse(updatedWarehouse);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _warehouseRepository.DeleteAsync(id);
    }

    private static void ValidateRequest(WarehouseRequestDto request)
    {
        if (request is null)
            throw new InvalidOperationException("Request cannot be null.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidOperationException("Warehouse name is required.");

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new InvalidOperationException("Warehouse code is required.");
    }

    private static WarehouseResponseDto MapToResponse(Warehouse warehouse)
    {
        return new WarehouseResponseDto
        {
            IdWarehouse = warehouse.IdWarehouse,
            Name = warehouse.Name,
            Code = warehouse.Code,
            Location = warehouse.Location,
            IsMain = warehouse.IsMain,
            Active = warehouse.Active
        };
    }
}
