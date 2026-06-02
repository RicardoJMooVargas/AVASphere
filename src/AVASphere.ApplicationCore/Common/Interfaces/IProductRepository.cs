using AVASphere.ApplicationCore.Common.DTOs.ProductDTOs;
using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.ApplicationCore.Common.Entities.Products;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IProductRepository
{
    Task<Product> CreateProductsAsync(Product product);
    Task CreateProductsBulkAsync(List<Product> products);
    Task<Product> UpdateProductsAsync(Product product);
    Task<bool> DeleteProductsAsync(int id);
    Task<Product?> GetByIdProductsAsync(int idProduct, ProductFilterDto? filters = null);
    Task<Product?> GetByPrincipalCodeAsync(string principalCode);
    Task<Product?> GetByMainNameAsync(string mainName);
    Task<List<Product>> GetByPrincipalCodesAsync(IEnumerable<string> principalCodes);
    Task<IEnumerable<Product>> GetAllProductsAsync(ProductFilterDto? filters = null, PaginationDto? pagination = null);

    /// <summary>
    /// Obtiene el total de productos con filtros (optimizado, sin cargar datos)
    /// </summary>
    Task<int> GetProductCountAsync(ProductFilterDto? filters = null);

    Task<Supplier?> GetSupplierByNameAsync(string name);
    Task<int?> FindPropertyValueIdAsync(string propertyName, string propertyValue);
    Task CreateProductPropertyAsync(int idProduct, int idPropertyValue);
    Task<int> GetOrCreatePropertyValueIdAsync(string propertyName, string propertyValue);
    
    // Métodos para optimización de importación masiva
    Task<Dictionary<string, Supplier>> GetAllSuppliersAsync();
    Task<Dictionary<string, int>> GetPropertyValueIdsByPropertyNameAsync(string propertyName);
}
