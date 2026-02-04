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

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Products.AnyAsync(p => p.IdProduct == id);
    }
}