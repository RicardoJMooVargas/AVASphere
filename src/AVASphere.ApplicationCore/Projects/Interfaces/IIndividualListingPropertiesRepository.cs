using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Projects.Entities.General;

namespace AVASphere.ApplicationCore.Projects.Interfaces;

public interface IIndividualListingPropertiesRepository
{
    // Create
    Task<IndividualListingProperties> CreateAsync(IndividualListingProperties individualListingProperties);
    
    // Read
    Task<IndividualListingProperties?> GetByIdAsync(int id);
    Task<IEnumerable<IndividualListingProperties>> GetAllAsync();
    Task<IEnumerable<IndividualProjectQuote>> GetByIndividualProjectQuoteAsync(int individualProjectQuoteId);
    Task<IEnumerable<ProductProperties>> GetByProductPropertiesAsync(int productPropertiesId);
    Task<bool> ExistsAsync(int id);
    
    // Update
    Task<IndividualListingProperties> UpdateAsync(IndividualListingProperties individualListingProperties);
    
    // Delete
    Task<bool> DeleteAsync(int id);

}