using AVASphere.ApplicationCore.Projects.DTOs;

namespace AVASphere.ApplicationCore.Projects.Interfaces;

public interface IIndividualListingPropertiesService
{
    // Create
    Task<IndividualListingPropertiesResponsetDto> CreateAsync(IndividualListingPropertiesRequestDto individualListingPropertiesRequest);
    
    // Read
    Task<IndividualListingPropertiesResponsetDto?> GetByIdAsync(int id);
    Task<IEnumerable<ProjectCategoryResponseDto>> GetAllAsync();
}