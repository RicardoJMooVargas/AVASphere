using AVASphere.ApplicationCore.Inventory.Entities.General;
using AVASphere.ApplicationCore.Inventory.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Inventory.Repository;

public class LocationDetailsRepository : ILocationDetailsRepository
{
    private readonly MasterDbContext _context;

    public LocationDetailsRepository(MasterDbContext context)
    {
        _context = context;
    }

    // Create
    public async Task<LocationDetails> CreateAsync(LocationDetails locationDetails)
    {
        if (locationDetails is null)
            throw new ArgumentNullException(nameof(locationDetails));

        _context.Set<LocationDetails>().Add(locationDetails);
        await _context.SaveChangesAsync();
        return locationDetails;
    }

    // Read
    public async Task<LocationDetails?> GetByIdAsync(int idLocationDetails)
    {
        return await _context.Set<LocationDetails>()
            .Include(ld => ld.Area)
            .Include(ld => ld.StorageStructure)
            .AsNoTracking()
            .FirstOrDefaultAsync(ld => ld.IdLocationDetails == idLocationDetails);
    }

    public async Task<IEnumerable<LocationDetails>> GetAllAsync()
    {
        return await _context.Set<LocationDetails>()
            .Include(ld => ld.Area)
            .Include(ld => ld.StorageStructure)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<LocationDetails>> GetByAreaIdAsync(int idArea)
    {
        return await _context.Set<LocationDetails>()
            .Where(ld => ld.IdArea == idArea)
            .Include(ld => ld.Area)
            .Include(ld => ld.StorageStructure)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<LocationDetails>> GetByStorageStructureIdAsync(int idStorageStructure)
    {
        return await _context.Set<LocationDetails>()
            .Where(ld => ld.IdStorageStructure == idStorageStructure)
            .Include(ld => ld.Area)
            .Include(ld => ld.StorageStructure)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<LocationDetails?> GetByLocationParametersAsync(int idArea, int idStorageStructure, string section, int verticalLevel)
    {
        if (string.IsNullOrWhiteSpace(section))
            throw new ArgumentException("Section cannot be null or empty.", nameof(section));

        return await _context.Set<LocationDetails>()
            .Include(ld => ld.Area)
            .Include(ld => ld.StorageStructure)
            .AsNoTracking()
            .FirstOrDefaultAsync(ld => 
                ld.IdArea == idArea && 
                ld.IdStorageStructure == idStorageStructure &&
                ld.Section == section &&
                ld.VerticalLevel == verticalLevel);
    }

    public async Task<bool> ExistsAsync(int idLocationDetails)
    {
        return await _context.Set<LocationDetails>()
            .AnyAsync(ld => ld.IdLocationDetails == idLocationDetails);
    }

    // Update
    public async Task<LocationDetails> UpdateAsync(LocationDetails locationDetails)
    {
        if (locationDetails is null)
            throw new ArgumentNullException(nameof(locationDetails));

        var tracked = await _context.Set<LocationDetails>().FindAsync(locationDetails.IdLocationDetails);
        if (tracked == null)
        {
            _context.Set<LocationDetails>().Update(locationDetails);
        }
        else
        {
            _context.Entry(tracked).CurrentValues.SetValues(locationDetails);
        }

        await _context.SaveChangesAsync();
        return locationDetails;
    }

    // Delete
    public async Task<bool> DeleteAsync(int idLocationDetails)
    {
        var entity = await _context.Set<LocationDetails>().FindAsync(idLocationDetails);
        if (entity == null)
            return false;

        _context.Set<LocationDetails>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
