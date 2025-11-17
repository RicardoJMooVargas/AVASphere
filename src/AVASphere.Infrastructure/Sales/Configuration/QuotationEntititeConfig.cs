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
        entity.HasKey(q => q.QuotationId);
        entity.Property(q => q.QuotationId)
            .HasColumnName("IdQuotation")
            .ValueGeneratedOnAdd();

        entity.Property(q => q.SaleDate)
            .HasColumnName("SaleDate")
            .IsRequired();

        entity.Property(q => q.Status)
            .HasColumnName("Status")
            .IsRequired()
            .HasMaxLength(50);

        // SalesExecutives: guardado como JSONB (lista de strings)
        entity.Property(q => q.SalesExecutives)
            .HasColumnName("SalesExecutives")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        entity.Property(q => q.Folio)
            .HasColumnName("Folio");

        entity.Property(q => q.CustomerId)
            .HasColumnName("IdCustomer")
            .IsRequired();

        entity.Property(q => q.GeneralComment)
            .HasColumnName("GeneralComment")
            .HasColumnType("text");

        entity.Property(q => q.Followups)
            .HasColumnName("FollowupsJson")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        // NUEVO: Lista simplificada de productos (JSONB)
        entity.Property(q => q.Products)
            .HasColumnName("ProductsJson")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        // NUEVOS CAMPOS: Vinculación con venta
        entity.Property(q => q.SaleId)
            .HasColumnName("SaleId")
            .HasMaxLength(100);

        entity.Property(q => q.SaleFolio)
            .HasColumnName("SaleFolio")
            .HasMaxLength(100);

        // FK a ConfigSys (si es necesaria)
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
              .HasForeignKey(q => q.CustomerId)
              .HasConstraintName("CustomerId")
              .OnDelete(DeleteBehavior.Restrict);

        // Relación con ConfigSys
        entity.HasOne(q => q.ConfigSys)
              .WithMany(c => c.Quotations)
              .HasForeignKey(q => q.IdConfigSys)
              .HasConstraintName("FKIdConfigSys")
              .OnDelete(DeleteBehavior.Restrict);
    }
}