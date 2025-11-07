
using System.Security.Cryptography.X509Certificates;
using AVASphere.ApplicationCore.Common.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Common.Configuration;

public class CustomerEntitieConfig : IEntityTypeConfiguration<Customer>
{
       public void Configure(EntityTypeBuilder<Customer> entity)
       {
              entity.ToTable("Customers");

              // Clave primaria
              entity.HasKey(c => c.IdCustomer)
                     .HasName("PK_Customers_IdCustomer");

              entity.Property(c => c.IdCustomer)
                     .HasColumnName("IdCustomer")
                     .ValueGeneratedOnAdd();

              // Configuración de propiedades básicas
              entity.Property(c => c.ExternalId)
                     .HasColumnName("ExternalId")
                     .IsRequired();

              entity.Property(c => c.Name)
                     .HasColumnName("Name")
                     .HasMaxLength(100)
                     .IsRequired(false);

              entity.Property(c => c.LastName)
                     .HasColumnName("LastName")
                     .HasMaxLength(100)
                     .IsRequired(false);

              entity.Property(c => c.PhoneNumber)
                     .HasColumnName("PhoneNumber")
                     .IsRequired();

              entity.Property(c => c.Email)
                     .HasColumnName("Email")
                     .HasMaxLength(255)
                     .IsRequired(false);

              entity.Property(c => c.TaxId)
                     .HasColumnName("TaxId")
                     .HasMaxLength(50)
                     .IsRequired(false);

              // Configuración de propiedades JSON para PostgreSQL
              entity.Property(c => c.SettingsCustomerJson)
                     .HasColumnName("SettingsCustomerJson")
                     .HasDefaultValueSql("'{}'::jsonb")
                     .HasColumnType("jsonb")
                     .IsRequired(false);

              entity.Property(c => c.DirectionJson)
                     .HasColumnName("DirectionJson")
                     .HasDefaultValueSql("'{}'::jsonb")
                     .HasColumnType("jsonb")
                     .IsRequired();

              entity.Property(c => c.PaymentMethodsJson)
                     .HasColumnName("PaymentMethodsJson")
                     .HasDefaultValueSql("'{}'::jsonb")
                     .HasColumnType("jsonb")
                     .IsRequired(false);

              entity.Property(c => c.PaymentTermsJson)
                     .HasColumnName("PaymentTermsJson")
                     .HasDefaultValueSql("'{}'::jsonb")
                     .HasColumnType("jsonb")
                     .IsRequired(false);

              // Índices para mejorar el rendimiento
              entity.HasIndex(c => c.ExternalId)
                     .HasDatabaseName("IX_Customers_ExternalId")
                     .IsUnique();

              entity.HasIndex(c => c.Email)
                     .HasDatabaseName("IX_Customers_Email");

              entity.HasIndex(c => c.TaxId)
                     .HasDatabaseName("IX_Customers_TaxId");

              // Índices para las propiedades JSON (PostgreSQL específico)
              entity.HasIndex(c => c.SettingsCustomerJson)
                     .HasDatabaseName("IX_Customers_SettingsCustomerJson")
                     .HasMethod("gin");

              entity.HasIndex(c => c.DirectionJson)
                     .HasDatabaseName("IX_Customers_DirectionJson")
                     .HasMethod("gin");

              // Relación 1:N -> Customer (1) : Quotations (N)
              entity.HasMany(c => c.Quotations)
                     .WithOne(q => q.Customer)
                     .HasForeignKey(q => q.CustomerId)
                     .HasConstraintName("FK_Quotations_Customers_IdCustomer")
                     .OnDelete(DeleteBehavior.Restrict);
       }
}