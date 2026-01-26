using AVASphere.ApplicationCore.Inventory.Entities.General;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Inventory.Repository;

public class WarehouseTransferRepository : IWarehouseTransferRepository
{
    private readonly MasterDbContext _context;

    public WarehouseTransferRepository(MasterDbContext context)
    {
        _context = context;
    }

    // Create
    public async Task<WarehouseTransfer> CreateAsync(WarehouseTransfer warehouseTransfer)
    {
        if (warehouseTransfer is null)
            throw new ArgumentNullException(nameof(warehouseTransfer));

        _context.Set<WarehouseTransfer>().Add(warehouseTransfer);
        await _context.SaveChangesAsync();
        return warehouseTransfer;
    }

    // Read
    public async Task<WarehouseTransfer?> GetByIdAsync(int idWarehouseTransfer)
    {
        return await _context.Set<WarehouseTransfer>()
            .AsNoTracking()
            .FirstOrDefaultAsync(wt => wt.IdWarehouseTransfer == idWarehouseTransfer);
    }

    public async Task<WarehouseTransfer?> GetByIdWithDetailsAsync(int idWarehouseTransfer)
    {
        return await _context.Set<WarehouseTransfer>()
            .Include(wt => wt.WarehouseTransferDetails)
                .ThenInclude(wtd => wtd.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(wt => wt.IdWarehouseTransfer == idWarehouseTransfer);
    }

    public async Task<IEnumerable<WarehouseTransfer>> GetAllAsync()
    {
        return await _context.Set<WarehouseTransfer>()
            .AsNoTracking()
            .OrderByDescending(wt => wt.TransferDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WarehouseTransfer>> GetAllWithDetailsAsync()
    {
        return await _context.Set<WarehouseTransfer>()
            .Include(wt => wt.WarehouseTransferDetails)
                .ThenInclude(wtd => wtd.Product)
            .AsNoTracking()
            .OrderByDescending(wt => wt.TransferDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WarehouseTransfer>> GetByStatusAsync(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status cannot be null or empty.", nameof(status));

        return await _context.Set<WarehouseTransfer>()
            .AsNoTracking()
            .Where(wt => wt.Status == status)
            .OrderByDescending(wt => wt.TransferDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WarehouseTransfer>> GetByWarehouseFromAsync(int idWarehouseFrom)
    {
        return await _context.Set<WarehouseTransfer>()
            .AsNoTracking()
            .Where(wt => wt.IdWarehouseFrom == idWarehouseFrom)
            .OrderByDescending(wt => wt.TransferDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WarehouseTransfer>> GetByWarehouseToAsync(int idWarehouseTo)
    {
        return await _context.Set<WarehouseTransfer>()
            .AsNoTracking()
            .Where(wt => wt.IdWarehouseTo == idWarehouseTo)
            .OrderByDescending(wt => wt.TransferDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WarehouseTransfer>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<WarehouseTransfer>()
            .AsNoTracking()
            .Where(wt => wt.TransferDate >= startDate.Ticks && wt.TransferDate <= endDate.Ticks)
            .OrderByDescending(wt => wt.TransferDate)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int idWarehouseTransfer)
    {
        return await _context.Set<WarehouseTransfer>()
            .AnyAsync(wt => wt.IdWarehouseTransfer == idWarehouseTransfer);
    }

    // Update
    public async Task<WarehouseTransfer> UpdateAsync(WarehouseTransfer warehouseTransfer)
    {
        if (warehouseTransfer is null)
            throw new ArgumentNullException(nameof(warehouseTransfer));

        var existingTransfer = await _context.Set<WarehouseTransfer>()
            .FirstOrDefaultAsync(wt => wt.IdWarehouseTransfer == warehouseTransfer.IdWarehouseTransfer);

        if (existingTransfer is null)
            throw new KeyNotFoundException($"WarehouseTransfer with ID {warehouseTransfer.IdWarehouseTransfer} not found.");

        _context.Entry(existingTransfer).CurrentValues.SetValues(warehouseTransfer);
        await _context.SaveChangesAsync();

        return existingTransfer;
    }

    public async Task<bool> UpdateStatusAsync(int idWarehouseTransfer, string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status cannot be null or empty.", nameof(status));

        var transfer = await _context.Set<WarehouseTransfer>()
            .FirstOrDefaultAsync(wt => wt.IdWarehouseTransfer == idWarehouseTransfer);

        if (transfer is null)
            return false;

        transfer.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }

    // Delete
    public async Task<bool> DeleteAsync(int idWarehouseTransfer)
    {
        var transfer = await _context.Set<WarehouseTransfer>()
            .Include(wt => wt.WarehouseTransferDetails)
            .FirstOrDefaultAsync(wt => wt.IdWarehouseTransfer == idWarehouseTransfer);

        if (transfer is null)
            return false;

        _context.Set<WarehouseTransfer>().Remove(transfer);
        await _context.SaveChangesAsync();
        return true;
    }
}

