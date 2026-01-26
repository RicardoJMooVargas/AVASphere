﻿using AVASphere.ApplicationCore.Common.Entities.Products;
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
        
        // Relación 1-N con Inventory
        entity.HasMany(p => p.Inventories)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.IdProduct)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relación 1-N con PhysicalInventoryDetail
        entity.HasMany(p => p.PhysicalInventoryDetails)
            .WithOne(pid => pid.Product)
            .HasForeignKey(pid => pid.IdProduct)
            .OnDelete(DeleteBehavior.Restrict);
        
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