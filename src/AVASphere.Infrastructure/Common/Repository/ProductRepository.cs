using AVASphere.ApplicationCore.Common.Entities.Products;
using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Common.Repository;

public class ProductRepository : IProductRepository
{
    private readonly MasterDbContext _context;

    public ProductRepository(MasterDbContext context)
    {
        _context = context;
    }

    public async Task<Product> CreateProductsAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateProductsAsync(Product product)
    {
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

    public async Task<Product?> GetByIdProductsAsync(int idProduct)
    {
        return await _context.Products
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.IdProduct == idProduct);
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Supplier)
            .ToListAsync();
    }
}