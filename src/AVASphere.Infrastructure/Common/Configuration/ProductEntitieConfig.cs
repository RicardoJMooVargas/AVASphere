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
        
        entity.Property(e => e.SupplierName)
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
        
        // Json Relacionado con Modelo
        entity.OwnsMany(p => p.CodeJson, ca =>
        {
            ca.ToJson();
            ca.Property(c => c.Index).HasColumnName("CodeJson_Index");
            ca.Property(c => c.Type).HasColumnName("CodeJson_Type");
            ca.Property(c => c.Code).HasColumnName("CodeJson_Code");
        });
        
        // Json Relacionado con Modelo
        entity.OwnsMany(p => p.CostsJson, ca =>
        {
            ca.ToJson();
            ca.Property(c => c.Index).HasColumnName("CostsJson_Index");
            ca.Property(c => c.Amount).HasColumnName("CostsJson_Amount");
            ca.Property(c => c.Type).HasColumnName("CostsJson_Type");
        });
        
        // Json Relacionado con Modelo
        entity.OwnsMany(p => p.CategoriesJsons, ca =>
        {
            ca.ToJson();
            ca.Property(c => c.Index).HasColumnName("CategoriesJson_Index");
            ca.Property(c => c.Name).HasColumnName("CategoriesJson_Name");
            ca.Property(c => c.NormalizedName).HasColumnName("CategoriesJson_NormalizedName");
        });
        
        // Json Relacionado con Modelo
        entity.OwnsMany(p => p.SolutionsJsons, ca =>
        {
            ca.ToJson();
            ca.Property(s => s.Index).HasColumnName("SolutionsJson_Index");
            ca.Property(s => s.Name).HasColumnName("SolutionsJson_Name");
            ca.Property(s => s.NormalizedName).HasColumnName("SolutionsJson_NormalizedName");
        });
        
    }
}