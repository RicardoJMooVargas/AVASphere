using AVASphere.ApplicationCore.Common.DTOs.ProductDTOs;
using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Common.Repository;

public class ProductRepository : IProductRepository
{
    private readonly MasterDbContext _context;
    private readonly ISupplierRepository _supplierRepository;

    public ProductRepository(MasterDbContext context, ISupplierRepository supplierRepository)
    {
        _context = context;
        _supplierRepository = supplierRepository;
    }

    public async Task<Product> CreateProductsAsync(Product product)
    {
        // Verificar si el proveedor existe usando el repositorio
        var supplierExists = await _supplierRepository.ExistsAsync(product.IdSupplier);
        if (!supplierExists)
        {
            throw new KeyNotFoundException($"El proveedor con ID {product.IdSupplier} no existe.");
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateProductsAsync(Product product)
    {
        // Verificar si el proveedor existe usando el repositorio
        var supplierExists = await _supplierRepository.ExistsAsync(product.IdSupplier);
        if (!supplierExists)
        {
            throw new KeyNotFoundException($"El proveedor con ID {product.IdSupplier} no existe.");
        }

        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<bool> DeleteProductsAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return false;
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Product?> GetByIdProductsAsync(int idProduct, ProductFilterDto? filters = null)
    {
        var query = _context.Products
            .Include(p => p.Supplier)
            .Include(p => p.ProductProperties)
                .ThenInclude(pp => pp.PropertyValue)
                    .ThenInclude(pv => pv.Property)
            .Where(p => p.IdProduct == idProduct);

        // Aplicar filtros adicionales si existen
        if (filters != null)
        {
            if (!string.IsNullOrEmpty(filters.MainName))
                query = query.Where(p => p.MainName.Contains(filters.MainName));

            if (filters.IdSupplier.HasValue && filters.IdSupplier.Value > 0)
                query = query.Where(p => p.IdSupplier == filters.IdSupplier.Value);

            if (!string.IsNullOrEmpty(filters.SupplierName))
                query = query.Where(p => p.Supplier!.Name!.Contains(filters.SupplierName));

            // Filtro por ID de Property
            if (filters.IdProperty.HasValue && filters.IdProperty.Value > 0)
                query = query.Where(p => p.ProductProperties.Any(pp => pp.PropertyValue.IdProperty == filters.IdProperty.Value));

            // Filtro por nombre de Property
            if (!string.IsNullOrEmpty(filters.PropertyName))
                query = query.Where(p => p.ProductProperties.Any(pp => pp.PropertyValue.Property.Name!.Contains(filters.PropertyName)));

            // Filtro por ID de PropertyValue
            if (filters.IdPropertyValue.HasValue && filters.IdPropertyValue.Value > 0)
                query = query.Where(p => p.ProductProperties.Any(pp => pp.IdPropertyValue == filters.IdPropertyValue.Value));

            // Filtro por valor de PropertyValue
            if (!string.IsNullOrEmpty(filters.PropertyValue))
                query = query.Where(p => p.ProductProperties.Any(pp => pp.PropertyValue.Value!.Contains(filters.PropertyValue)));

            // Filtros dinámicos por propiedades
            if (filters.Properties != null && filters.Properties.Any())
            {
                foreach (var propertyFilter in filters.Properties)
                {
                    var propertyName = propertyFilter.Key;
                    var propertyValue = propertyFilter.Value;

                    query = query.Where(p =>
                        p.ProductProperties.Any(pp =>
                            pp.PropertyValue.Property.Name == propertyName &&
                            (pp.PropertyValue.Value!.Contains(propertyValue) ||
                             (pp.CustomValue != null && pp.CustomValue.Contains(propertyValue)))
                        )
                    );
                }
            }
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync(ProductFilterDto? filters = null)
    {
        var query = _context.Products
            .Include(p => p.Supplier)
            .Include(p => p.ProductProperties)
                .ThenInclude(pp => pp.PropertyValue)
                    .ThenInclude(pv => pv.Property)
            .AsQueryable();

        // Aplicar filtros si existen
        if (filters != null)
        {
            if (!string.IsNullOrEmpty(filters.MainName))
                query = query.Where(p => p.MainName.Contains(filters.MainName));

            if (filters.IdSupplier.HasValue && filters.IdSupplier.Value > 0)
                query = query.Where(p => p.IdSupplier == filters.IdSupplier.Value);

            if (!string.IsNullOrEmpty(filters.SupplierName))
                query = query.Where(p => p.Supplier!.Name!.Contains(filters.SupplierName));

            // Filtro por ID de Property
            if (filters.IdProperty.HasValue && filters.IdProperty.Value > 0)
                query = query.Where(p => p.ProductProperties.Any(pp => pp.PropertyValue.IdProperty == filters.IdProperty.Value));

            // Filtro por nombre de Property
            if (!string.IsNullOrEmpty(filters.PropertyName))
                query = query.Where(p => p.ProductProperties.Any(pp => pp.PropertyValue.Property.Name!.Contains(filters.PropertyName)));

            // Filtro por ID de PropertyValue
            if (filters.IdPropertyValue.HasValue && filters.IdPropertyValue.Value > 0)
                query = query.Where(p => p.ProductProperties.Any(pp => pp.IdPropertyValue == filters.IdPropertyValue.Value));

            // Filtro por valor de PropertyValue
            if (!string.IsNullOrEmpty(filters.PropertyValue))
                query = query.Where(p => p.ProductProperties.Any(pp => pp.PropertyValue.Value!.Contains(filters.PropertyValue)));

            // Filtros dinámicos por propiedades
            if (filters.Properties != null && filters.Properties.Any())
            {
                foreach (var propertyFilter in filters.Properties)
                {
                    var propertyName = propertyFilter.Key;
                    var propertyValue = propertyFilter.Value;

                    query = query.Where(p =>
                        p.ProductProperties.Any(pp =>
                            pp.PropertyValue.Property.Name == propertyName &&
                            (pp.PropertyValue.Value!.Contains(propertyValue) ||
                             (pp.CustomValue != null && pp.CustomValue.Contains(propertyValue)))
                        )
                    );
                }
            }
        }

        return await query.ToListAsync();
    }

    public async Task<Supplier?> GetSupplierByNameAsync(string name)
    {
        return await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Name!.ToLower() == name.ToLower());
    }

    public async Task<int?> FindPropertyValueIdAsync(string propertyName, string propertyValue)
    {
        return await _context.PropertyValues
            .Where(pv =>
                pv.Property.Name!.ToLower() == propertyName.ToLower() &&
                pv.Value!.ToLower() == propertyValue.ToLower())
            .Select(pv => (int?)pv.IdPropertyValue)
            .FirstOrDefaultAsync();
    }

    public async Task CreateProductPropertyAsync(int idProduct, int idPropertyValue)
    {
        var productProperty = new ProductProperties
        {
            IdProduct = idProduct,
            IdPropertyValue = idPropertyValue
        };

        _context.ProductProperties.Add(productProperty);
        await _context.SaveChangesAsync();
    }
}