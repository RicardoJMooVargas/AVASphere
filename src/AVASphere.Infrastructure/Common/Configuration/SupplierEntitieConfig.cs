//ACTUALIZADO A LA VERSION 0.2 DE LA DB
using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Common.Configuration;

public class SupplierEntitieConfig : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> entity)
    {
        entity.ToTable("Supplier");
        entity.HasKey(e => e.IdSupplier);

        entity.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(e => e.CompanyName)
            .HasMaxLength(200);

        entity.Property(e => e.TaxId)
            .HasMaxLength(50);

        entity.Property(e => e.PersonType)
            .HasMaxLength(50);

        entity.Property(e => e.BusinessId)
            .HasMaxLength(100);

        entity.Property(e => e.CurrencyCoin)
            .HasMaxLength(10);

        entity.Property(e => e.RegistrationDate)
            .IsRequired();

        entity.Property(e => e.Observations)
            .HasMaxLength(500);

        // Configuración de propiedades JSON para PostgreSQL
        entity.Property(e => e.ContactsJson)
            .HasColumnName("ContactsJson")
            .HasDefaultValueSql("'{}'::jsonb")
            .HasColumnType("jsonb")
            .IsRequired(false);

        entity.Property(e => e.PaymentTermsJson)
            .HasColumnName("PaymentTermsJson")
            .HasDefaultValueSql("'{}'::jsonb")
            .HasColumnType("jsonb")
            .IsRequired(false);

        entity.Property(e => e.PaymentMethodsJson)
            .HasColumnName("PaymentMethodsJson")
            .HasDefaultValueSql("'{}'::jsonb")
            .HasColumnType("jsonb")
            .IsRequired(false);

        // Configuración de relación
        entity.HasMany(s => s.Product)
            .WithOne(p => p.Supplier)
            .HasForeignKey(p => p.IdSupplier)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

