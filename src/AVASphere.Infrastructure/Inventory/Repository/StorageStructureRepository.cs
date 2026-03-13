using AVASphere.ApplicationCore.Inventory.Entities.General;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Inventory.Repository;

public class StorageStructureRepository : IStorageStructureRepository
{
    private readonly MasterDbContext _context;

    public StorageStructureRepository(MasterDbContext context)
    {
        _context = context;
    }

    // Create
    public async Task<StorageStructure> CreateAsync(StorageStructure storageStructure)
    {
        if (storageStructure is null)
            throw new ArgumentNullException(nameof(storageStructure));

        _context.Set<StorageStructure>().Add(storageStructure);
        await _context.SaveChangesAsync();
        return storageStructure;
    }

    // Read
    public async Task<StorageStructure?> GetByIdAsync(int idStorageStructure)
    {
        return await _context.Set<StorageStructure>()
            .AsNoTracking()
            .FirstOrDefaultAsync(ss => ss.IdStorageStructure == idStorageStructure);
    }

    public async Task<StorageStructure?> GetByCodeAsync(string codeRack)
    {
        if (string.IsNullOrWhiteSpace(codeRack))
            throw new ArgumentException("Code rack cannot be null or empty.", nameof(codeRack));

        return await _context.Set<StorageStructure>()
            .AsNoTracking()
            .FirstOrDefaultAsync(ss => ss.CodeRack == codeRack);
    }

    public async Task<IEnumerable<StorageStructure>> GetAllAsync()
    {
        return await _context.Set<StorageStructure>()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<StorageStructure>> GetByWarehouseAsync(int idWarehouse)
    {
        return await _context.Set<StorageStructure>()
            .AsNoTracking()
            .Where(ss => ss.IdWarehouse == idWarehouse)
            .ToListAsync();
    }

    public async Task<IEnumerable<StorageStructure>> GetByAreaIdAsync(int idArea)
    {
        return await _context.Set<StorageStructure>()
            .AsNoTracking()
            .Where(ss => ss.IdArea == idArea)
            .ToListAsync();
    }

    public async Task<IEnumerable<StorageStructure>> GetByWarehouseAndAreaAsync(int idWarehouse, int idArea)
    {
        return await _context.Set<StorageStructure>()
            .AsNoTracking()
            .Where(ss => ss.IdWarehouse == idWarehouse)
            .Where(ss => ss.LocationDetails.Any(ld => ld.IdArea == idArea))
            .ToListAsync();
    }

    public async Task<IEnumerable<StorageStructure>> GetFilteredAsync(
        int? idStorageStructure = null,
        string? codeRack = null,
        bool? oneSection = null,
        bool? hasLevel = null,
        bool? hasSubLevel = null,
        int? idWarehouse = null,
        int? idArea = null)
    {
        var query = _context.Set<StorageStructure>()
            .Include(ss => ss.Warehouse)
            .Include(ss => ss.Area)
            .Include(ss => ss.LocationDetails)
            .AsQueryable();

        if (idStorageStructure.HasValue)
            query = query.Where(ss => ss.IdStorageStructure == idStorageStructure.Value);

        if (!string.IsNullOrEmpty(codeRack))
            query = query.Where(ss => ss.CodeRack.Contains(codeRack));

        if (oneSection.HasValue)
            query = query.Where(ss => ss.OneSection == oneSection.Value);

        if (hasLevel.HasValue)
            query = query.Where(ss => ss.HasLevel == hasLevel.Value);

        if (hasSubLevel.HasValue)
            query = query.Where(ss => ss.HasSubLevel == hasSubLevel.Value);

        if (idWarehouse.HasValue)
            query = query.Where(ss => ss.IdWarehouse == idWarehouse.Value);

        if (idArea.HasValue)
            query = query.Where(ss => ss.IdArea == idArea.Value);

        return await query
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int idStorageStructure)
    {
        return await _context.Set<StorageStructure>()
            .AnyAsync(ss => ss.IdStorageStructure == idStorageStructure);
    }

    public async Task<bool> ExistsByCodeAsync(string codeRack)
    {
        if (string.IsNullOrWhiteSpace(codeRack))
            return false;

        return await _context.Set<StorageStructure>()
            .AnyAsync(ss => ss.CodeRack == codeRack);
    }

    // Update
    public async Task<StorageStructure> UpdateAsync(StorageStructure storageStructure)
    {
        if (storageStructure is null)
            throw new ArgumentNullException(nameof(storageStructure));

        var tracked = await _context.Set<StorageStructure>().FindAsync(storageStructure.IdStorageStructure);
        if (tracked == null)
        {
            _context.Set<StorageStructure>().Update(storageStructure);
        }
        else
        {
            _context.Entry(tracked).CurrentValues.SetValues(storageStructure);
        }

        await _context.SaveChangesAsync();
        return storageStructure;
    }

    // Delete
    public async Task<bool> DeleteAsync(int idStorageStructure)
    {
        var entity = await _context.Set<StorageStructure>().FindAsync(idStorageStructure);
        if (entity == null)
            return false;

        _context.Set<StorageStructure>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
