using AVASphere.ApplicationCore.Common.DTOs.ProductDTOs;
using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.ApplicationCore.Common.Entities.Products;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IProductRepository
{
    Task<Product> CreateProductsAsync(Product product);
    Task<Product> UpdateProductsAsync(Product product);
    Task<bool> DeleteProductsAsync(int id);
    Task<Product?> GetByIdProductsAsync(int idProduct, ProductFilterDto? filters = null);
    Task<IEnumerable<Product>> GetAllProductsAsync(ProductFilterDto? filters = null, PaginationDto? pagination = null);

    /// <summary>
    /// Obtiene el total de productos con filtros (optimizado, sin cargar datos)
    /// </summary>
    Task<int> GetProductCountAsync(ProductFilterDto? filters = null);

    Task<Supplier?> GetSupplierByNameAsync(string name);
    Task<int?> FindPropertyValueIdAsync(string propertyName, string propertyValue);
    Task CreateProductPropertyAsync(int idProduct, int idPropertyValue);
}