using AVASphere.ApplicationCore.Inventory.Entities.General;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Inventory.Repository;

public class WarehouseTransferDetailRepository : IWarehouseTransferDetailRepository
{
    private readonly MasterDbContext _context;

    public WarehouseTransferDetailRepository(MasterDbContext context)
    {
        _context = context;
    }

    // Create
    public async Task<WarehouseTransferDetail> CreateAsync(WarehouseTransferDetail transferDetail)
    {
        if (transferDetail is null)
            throw new ArgumentNullException(nameof(transferDetail));

        _context.Set<WarehouseTransferDetail>().Add(transferDetail);
        await _context.SaveChangesAsync();
        return transferDetail;
    }

    public async Task<IEnumerable<WarehouseTransferDetail>> CreateBatchAsync(IEnumerable<WarehouseTransferDetail> transferDetails)
    {
        if (transferDetails is null)
            throw new ArgumentNullException(nameof(transferDetails));
            
        var detailsList = transferDetails.ToList();
        if (!detailsList.Any())
            throw new ArgumentException("Transfer details collection cannot be empty.", nameof(transferDetails));

        await _context.Set<WarehouseTransferDetail>().AddRangeAsync(detailsList);
        await _context.SaveChangesAsync();
        return detailsList;
    }

    // Read
    public async Task<WarehouseTransferDetail?> GetByIdAsync(int idTransferDetail)
    {
        return await _context.Set<WarehouseTransferDetail>()
            .Include(wtd => wtd.Product)
            .Include(wtd => wtd.WarehouseTransfer)
            .AsNoTracking()
            .FirstOrDefaultAsync(wtd => wtd.IdTransferDetail == idTransferDetail);
    }

    public async Task<IEnumerable<WarehouseTransferDetail>> GetAllAsync()
    {
        return await _context.Set<WarehouseTransferDetail>()
            .Include(wtd => wtd.Product)
            .Include(wtd => wtd.WarehouseTransfer)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<WarehouseTransferDetail>> GetByTransferIdAsync(int idWarehouseTransfer)
    {
        return await _context.Set<WarehouseTransferDetail>()
            .Include(wtd => wtd.Product)
            .AsNoTracking()
            .Where(wtd => wtd.IdWarehouseTransfer == idWarehouseTransfer)
            .ToListAsync();
    }

    public async Task<IEnumerable<WarehouseTransferDetail>> GetByProductIdAsync(int idProduct)
    {
        return await _context.Set<WarehouseTransferDetail>()
            .Include(wtd => wtd.Product)
            .Include(wtd => wtd.WarehouseTransfer)
            .AsNoTracking()
            .Where(wtd => wtd.IdProduct == idProduct)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int idTransferDetail)
    {
        return await _context.Set<WarehouseTransferDetail>()
            .AnyAsync(wtd => wtd.IdTransferDetail == idTransferDetail);
    }

    // Update
    public async Task<WarehouseTransferDetail> UpdateAsync(WarehouseTransferDetail transferDetail)
    {
        if (transferDetail is null)
            throw new ArgumentNullException(nameof(transferDetail));

        var existingDetail = await _context.Set<WarehouseTransferDetail>()
            .FirstOrDefaultAsync(wtd => wtd.IdTransferDetail == transferDetail.IdTransferDetail);

        if (existingDetail is null)
            throw new KeyNotFoundException($"WarehouseTransferDetail with ID {transferDetail.IdTransferDetail} not found.");

        _context.Entry(existingDetail).CurrentValues.SetValues(transferDetail);
        await _context.SaveChangesAsync();

        return existingDetail;
    }

    // Delete
    public async Task<bool> DeleteAsync(int idTransferDetail)
    {
        var transferDetail = await _context.Set<WarehouseTransferDetail>()
            .FirstOrDefaultAsync(wtd => wtd.IdTransferDetail == idTransferDetail);

        if (transferDetail is null)
            return false;

        _context.Set<WarehouseTransferDetail>().Remove(transferDetail);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteByTransferIdAsync(int idWarehouseTransfer)
    {
        var transferDetails = await _context.Set<WarehouseTransferDetail>()
            .Where(wtd => wtd.IdWarehouseTransfer == idWarehouseTransfer)
            .ToListAsync();

        if (!transferDetails.Any())
            return false;

        _context.Set<WarehouseTransferDetail>().RemoveRange(transferDetails);
        await _context.SaveChangesAsync();
        return true;
    }
}
