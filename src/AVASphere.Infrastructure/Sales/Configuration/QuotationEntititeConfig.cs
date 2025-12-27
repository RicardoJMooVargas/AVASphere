using AVASphere.ApplicationCore.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Sales.Configuration;

public class QuotationEntitieConfig : IEntityTypeConfiguration<Quotation>
{
    public void Configure(EntityTypeBuilder<Quotation> entity)
    {
        entity.ToTable("Quotations");

        // PK entero autoincremental
        entity.HasKey(q => q.IdQuotation);
        entity.Property(q => q.IdQuotation)
            .HasColumnName("IdQuotation")
            .ValueGeneratedOnAdd();
        
        entity.Property(q => q.IdCustomer)
            .HasColumnName("IdCustomer")
            .IsRequired();

        entity.Property(q => q.SaleDate)
            .HasColumnName("SaleDate")
            .IsRequired();

        entity.Property(q => q.Status)
            .HasColumnName("Status")
            .IsRequired()
            .HasConversion<int>();

        // SalesExecutives: guardado como JSONB (lista de strings)
        entity.Property(q => q.SalesExecutives)
            .HasColumnName("SalesExecutives")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        entity.Property(q => q.Folio)
            .HasColumnName("Folio");

        entity.Property(q => q.GeneralComment)
            .HasColumnName("GeneralComment")
            .HasColumnType("text");

        entity.Property(q => q.FollowupsJson)
            .HasColumnName("FollowupsJson")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        // NUEVO: Lista simplificada de productos (JSONB)
        entity.Property(q => q.ProductsJson)
            .HasColumnName("ProductsJson")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        // NUEVOS CAMPOS: Vinculación con venta
        entity.Property(q => q.LinkedSaleId)
            .HasColumnName("LinkedSaleId")
            .HasMaxLength(100);

        entity.Property(q => q.LinkedSaleFolio)
            .HasColumnName("LinkedSaleFolio")
            .HasMaxLength(100);

        // FK a ConfigSys
        entity.Property(q => q.IdConfigSys)
            .HasColumnName("IdConfigSys")
            .IsRequired();

        entity.Property(q => q.CreatedAt)
            .HasColumnName("CreatedAt");

        entity.Property(q => q.UpdatedAt)
            .HasColumnName("UpdatedAt");

        // Relación con Customer (bidireccional)
        entity.HasOne(q => q.Customer)
              .WithMany(c => c.Quotations)
              .HasForeignKey(q => q.IdCustomer)
              .HasConstraintName("FK_Quotations_Customers_IdCustomer")
              .OnDelete(DeleteBehavior.Restrict);

        // Relación con ConfigSys
        entity.HasOne(q => q.ConfigSys)
              .WithMany(c => c.Quotations)
              .HasForeignKey(q => q.IdConfigSys)
              .HasConstraintName("FK_Quotations_ConfigSys_IdConfigSys")
              .OnDelete(DeleteBehavior.Restrict);
    }
}