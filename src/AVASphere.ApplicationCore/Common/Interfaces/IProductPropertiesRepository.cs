using AVASphere.ApplicationCore.Common.Entities.Products;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IProductPropertiesRepository
{
    Task<ProductProperties> CreateProductPropertiesAsync(ProductProperties productProperties);
    Task<ProductProperties> UpdateProductPropertiesAsync(ProductProperties productProperties);
    Task<bool> DeleteProductPropertiesAsync(int id);
    Task<ProductProperties?> GetByIdProductPropertiesAsync(int id);
}