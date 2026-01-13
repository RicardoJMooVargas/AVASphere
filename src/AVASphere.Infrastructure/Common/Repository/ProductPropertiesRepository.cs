using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Common.Repository;

public class ProductPropertiesRepository : IProductPropertiesRepository
{
    private readonly MasterDbContext _context;

    public ProductPropertiesRepository(MasterDbContext context)
    {
        _context = context;
    }

    public async Task<ProductProperties> CreateProductPropertiesAsync(ProductProperties productProperties)
    {
        _context.ProductProperties.Add(productProperties);
        await _context.SaveChangesAsync();
        return productProperties;
    }

    public async Task<ProductProperties> UpdateProductPropertiesAsync(ProductProperties productProperties)
    {
        _context.ProductProperties.Update(productProperties);
        await _context.SaveChangesAsync();
        return productProperties;
    }

    public async Task<bool> DeleteProductPropertiesAsync(int id)
    {
        var productProperties = await _context.ProductProperties.FindAsync(id);
        if (productProperties == null) return false;

        _context.ProductProperties.Remove(productProperties);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<ProductProperties?> GetByIdProductPropertiesAsync(int id)
    {
        return await _context.ProductProperties
            .Include(pp => pp.Product)
            .Include(pp => pp.PropertyValue)
            .FirstOrDefaultAsync(pp => pp.IdProductProperties == id);
    }


}