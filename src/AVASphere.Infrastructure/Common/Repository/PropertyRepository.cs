using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.ApplicationCore.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace AVASphere.Infrastructure.Common.Repository;


public class PropertyRepository : IPropertyRepository
    {
        private readonly MasterDbContext _context;

        public PropertyRepository(MasterDbContext context)
        {
            _context = context;
        }

        public async Task<Property> CreateAsync(Property property)
        {
            _context.Properties.Add(property);
            await _context.SaveChangesAsync();
            return property;
        }

        public async Task<Property?> GetByIdAsync(int id)
        {
            return await _context.Properties
                .FirstOrDefaultAsync(pc => pc.IdProperty == id);
        }
    
        public async Task<Property?> GetByNameAsync(string name)
        {
            return await _context.Properties
                .FirstOrDefaultAsync(pc => pc.Name == name || pc.NormalizedName == name);
        }
   
        public async Task<IEnumerable<Property>> GetAllAsync()
        {
            return await _context.Properties
                .ToListAsync();
        }
    
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Properties.AnyAsync(pc => pc.IdProperty == id);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Properties.AnyAsync(pc => pc.Name == name || pc.NormalizedName == name);
        }

        public async Task<Property> UpdateAsync(Property property)
        {
            _context.Properties.Update(property);
            await _context.SaveChangesAsync();
            return property;
        }
    
        public async Task<bool> DeleteAsync(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null) return false;

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<IEnumerable<Property>> GetPropertyValuesByPropertyAsync(int propertyId)
        {
            return await _context.Properties
                .Where(pv => pv.IdProperty == propertyId)
                .ToListAsync();
        }
    }
