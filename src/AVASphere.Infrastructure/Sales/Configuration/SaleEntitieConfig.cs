using AVASphere.ApplicationCore.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Sales.Configuration;

public class SaleEntitieConfig : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> entity)
    {
        entity.ToTable("Sales");

        // PK entero autoincremental
        entity.HasKey(s => s.IdSale);
        entity.Property(s => s.IdSale)
            .HasColumnName("IdSale")
            .ValueGeneratedOnAdd();
        
        entity.Property(s => s.IdCustomer)
            .HasColumnName("IdCustomer")
            .IsRequired();


        entity.Property(s => s.SalesExecutive)
            .HasColumnName("SalesExecutive")
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(s => s.SaleDate)
            .HasColumnName("SaleDate")
            .IsRequired();

        entity.Property(s => s.Type)
            .HasColumnName("Type")
            .IsRequired()
            .HasMaxLength(50);
        
        entity.Property(s => s.Folio)
            .HasColumnName("Folio")
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(s => s.TotalAmount)
            .HasColumnName("TotalAmount")
            .HasPrecision(18, 2);

        entity.Property(s => s.DeliveryDriver)
            .HasColumnName("DeliveryDriver")
            .HasMaxLength(100);

        entity.Property(s => s.HomeDelivery)
            .HasColumnName("HomeDelivery")
            .HasDefaultValue(false);

        entity.Property(s => s.DeliveryDate)
            .HasColumnName("DeliveryDate");

        entity.Property(s => s.SatisfactionLevel)
            .HasColumnName("SatisfactionLevel")
            .HasDefaultValue(0);

        entity.Property(s => s.SatisfactionReason)
            .HasColumnName("SatisfactionReason")
            .HasMaxLength(500)
            .HasDefaultValue("not specified");

        entity.Property(s => s.Comment)
            .HasColumnName("Comment")
            .HasMaxLength(1000);

        entity.Property(s => s.AfterSalesFollowupDate)
            .HasColumnName("AfterSalesFollowupDate");

        // NUEVO: Lista de cotizaciones vinculadas (JSONB)
        entity.Property(s => s.LinkedQuotations)
            .HasColumnName("LinkedQuotationsJson")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        // NUEVO: Lista simplificada de productos (JSONB)
        entity.Property(s => s.ProductsJson)
            .HasColumnName("ProductsJson")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        // NUEVO: Datos de nota externa (JSONB)
        entity.Property(s => s.AuxNoteDataJson)
            .HasColumnName("AuxNoteDataJson")
            .HasColumnType("jsonb")
            .IsRequired(false);

        // FK a ConfigSys (si es necesaria)
        entity.Property(s => s.IdConfigSys)
            .HasColumnName("IdConfigSys")
            .IsRequired();

        entity.Property(s => s.CreatedAt)
            .HasColumnName("CreatedAt")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");


        entity.Property(s => s.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relación con Customer
        entity.HasOne(s => s.Customer)
              .WithMany(c => c.Sales)
              .HasForeignKey(s => s.IdCustomer)
              .HasConstraintName("FK_Sales_Customers_IdCustomer")
              .OnDelete(DeleteBehavior.Restrict);

        // Relación con ConfigSys
        entity.HasOne(s => s.ConfigSys)
              .WithMany(c => c.Sales)
              .HasForeignKey(s => s.IdConfigSys)
              .HasConstraintName("IdConfigSys")
              .OnDelete(DeleteBehavior.Restrict);
    }
}