﻿﻿using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Projects.Entities.General;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Common.Repository;


public class PropertyValueRepository : IPropertyValueRepository
{
    private readonly MasterDbContext _context;

    public PropertyValueRepository(MasterDbContext context)
    {
        _context = context;
    }

    public async Task<PropertyValue> CreateAsync(PropertyValue propertyValue)
    {
        _context.PropertyValues.Add(propertyValue);
        await _context.SaveChangesAsync();
        return propertyValue;
    }

    public async Task<PropertyValue?> GetByIdAsync(int id)
    {
        return await _context.PropertyValues
            .Include(pv => pv.Property)
            .FirstOrDefaultAsync(pv => pv.IdPropertyValue == id);
    }

    public async Task<PropertyValue?> GetByValueAsync(string value)
    {
        return await _context.PropertyValues
            .Include(pv => pv.Property)
            .FirstOrDefaultAsync(pv => pv.Value!.ToUpper() == value.ToUpper());
    }

    public async Task<IEnumerable<PropertyValue>> GetAllAsync()
    {
        return await _context.PropertyValues
            .Include(pv => pv.Property)
            .ToListAsync();
    }

    public async Task<IEnumerable<PropertyValue>> GetByPropertyAsync(int propertyId)
    {
        return await _context.PropertyValues
            .Include(pv => pv.Property)
            .Where(pv => pv.IdProperty == propertyId)
            .ToListAsync();
    }


    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.PropertyValues.AnyAsync(pv => pv.IdPropertyValue == id);
    }

    public async Task<bool> ExistsByValueAsync(string value)
    {
        return await _context.PropertyValues.AnyAsync(pv => pv.Value!.ToUpper() == value.ToUpper());
    }

    public async Task<PropertyValue> UpdateAsync(PropertyValue propertyValue)
    {
        _context.PropertyValues.Update(propertyValue);
        await _context.SaveChangesAsync();
        return propertyValue;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var propertyValue = await _context.PropertyValues.FindAsync(id);
        if (propertyValue == null) return false;

        _context.PropertyValues.Remove(propertyValue);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ProductProperties>> GetProductPropertiesByPropertyValueAsync(int propertyValueId)
    {
        return await _context.ProductProperties
            .Where(pp => pp.IdPropertyValue == propertyValueId)
            .ToListAsync();
    }

    public async Task<IEnumerable<IndividualListingProperties>> GetIndividualListingPropertiesByPropertyValueAsync(int propertyValueId)
    {
        // TODO: Esta funcionalidad necesita ser reimplementada con la nueva relación
        // PropertyValue -> ProductProperties -> IndividualListingProperties
        return await Task.FromResult(new List<IndividualListingProperties>());
        
        // return await _context.IndividualListingProperties
        //     .Where(ilp => ilp.ProductProperties.IdPropertyValue == propertyValueId)
        //     .ToListAsync();
    }
}