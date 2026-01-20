﻿using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Projects.Entities.General;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IPropertyValueRepository
{
    // Create
    Task<PropertyValue> CreateAsync(PropertyValue propertyValue);
    
    // Read
    Task<PropertyValue?> GetByIdAsync(int id);
    Task<PropertyValue?> GetByValueAsync(string value);
    Task<IEnumerable<PropertyValue>> GetAllAsync();
    Task<IEnumerable<PropertyValue>> GetByPropertyAsync(int propertyId);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByValueAsync(string value);
    
    // Update
    Task<PropertyValue> UpdateAsync(PropertyValue propertyValue);
    
    // Delete
    Task<bool> DeleteAsync(int id);
    
    // Relaciones
    Task<IEnumerable<ProductProperties>> GetProductPropertiesByPropertyValueAsync(int propertyValueId);
    Task<IEnumerable<IndividualListingProperties>> GetIndividualListingPropertiesByPropertyValueAsync(int propertyValueId);
    
}