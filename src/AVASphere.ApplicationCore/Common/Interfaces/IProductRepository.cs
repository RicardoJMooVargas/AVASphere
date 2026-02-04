using AVASphere.ApplicationCore.Common.Entities.Products;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IProductRepository
{
    Task<Product> CreateProductsAsync(Product product);
    Task<Product> UpdateProductsAsync(Product product);
    Task<bool> DeleteProductsAsync(int id);
    Task<Product?> GetByIdProductsAsync(int idProduct);
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<bool> ExistsAsync(int id);
}