//ACTUALIZADO A LA VERSION 0.2 DE LA DB
using AVASphere.ApplicationCore.Common.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Common.Configuration;

public class ProductEntitieConfig : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> entity)
    {
        entity.ToTable("Product");
        entity.HasKey(e => e.IdProduct);
        
        entity.Property(e => e.MainName)
            .IsRequired()
            .HasMaxLength(255);
        
        entity.Property(e => e.Unit)
            .IsRequired()
            .HasMaxLength(50);
        
        entity.Property(e => e.Description)
            .HasMaxLength(500);
        
        entity.Property(e => e.Quantity)
            .IsRequired();
        
        entity.Property(e => e.Taxes)
            .IsRequired();
        
        // FK a Supplier (requerida)
        entity.HasOne(p => p.Supplier)
            .WithMany(s => s.Product)
            .HasForeignKey(p => p.IdSupplier)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relación 1-N con ProductProperties
        entity.HasMany(p => p.ProductProperties)
            .WithOne(pp => pp.Product)
            .HasForeignKey(pp => pp.IdProduct)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relación 1-N con ListOfProductsToQuot
        entity.HasMany(p => p.ProductImages)
            .WithOne(lptq => lptq.Product)
            .HasForeignKey(lptq => lptq.IdProduct)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Las relaciones con entidades de inventario se configuran en sus respectivos EntityConfigs para evitar duplicados:
        // - Inventory: InventoryEntitieConfig
        // - PhysicalInventoryDetail: PhysicalInventoryDetailEntitieConfig  
        // - StockMovement: StockMovementEntitieConfig
        // - WarehouseTransferDetail: WarehouseTransferDetailEntitieConfig
        
        // Json Relacionado con Modelo
        entity.Property(e => e.CodeJson)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb")
            .IsRequired();
        
        // Json Relacionado con Modelo
        entity.Property(e => e.CostsJson)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb")
            .IsRequired();
        
        // Json Relacionado con Modelo
        entity.Property(e => e.CategoriesJsons)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb")
                .IsRequired();
        
        // Json Relacionado con Modelo
        entity.Property(e => e.SolutionsJsons)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb")
            .IsRequired();
    }
}