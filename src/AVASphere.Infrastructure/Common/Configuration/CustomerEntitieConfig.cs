
using AVASphere.ApplicationCore.Common.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Common.Configuration;

public class CustomerEntitieConfig : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers", "common");
        
        // Clave primaria
        builder.HasKey(c => c.IdCustomer)
               .HasName("PK_Customers_IdCustomer");
        
        builder.Property(c => c.IdCustomer)
               .HasColumnName("IdCustomer")
               .ValueGeneratedOnAdd();
        
        // Configuración de propiedades básicas
        builder.Property(c => c.ExternalId)
               .HasColumnName("ExternalId")
               .IsRequired();
        
        builder.Property(c => c.Name)
               .HasColumnName("Name")
               .HasMaxLength(100)
               .IsRequired(false);
        
        builder.Property(c => c.LastName)
               .HasColumnName("LastName")
               .HasMaxLength(100)
               .IsRequired(false);
        
        builder.Property(c => c.PhoneNumber)
               .HasColumnName("PhoneNumber")
               .IsRequired();
        
        builder.Property(c => c.Email)
               .HasColumnName("Email")
               .HasMaxLength(255)
               .IsRequired(false);
        
        builder.Property(c => c.TaxId)
               .HasColumnName("TaxId")
               .HasMaxLength(50)
               .IsRequired(false);
        
        // Configuración de propiedades JSON para PostgreSQL
        builder.Property(c => c.SettingsCustomerJson)
               .HasColumnName("SettingsCustomerJson")
               .HasDefaultValueSql("'{}'::jsonb") 
               .HasColumnType("jsonb")
               .IsRequired(false);
        
        builder.Property(c => c.DirectionJson)
               .HasColumnName("DirectionJson")
               .HasDefaultValueSql("'{}'::jsonb") 
               .HasColumnType("jsonb")
               .IsRequired();
        
        builder.Property(c => c.PaymentMethodsJson)
               .HasColumnName("PaymentMethodsJson")
               .HasDefaultValueSql("'{}'::jsonb") 
               .HasColumnType("jsonb")
               .IsRequired(false);
        
        builder.Property(c => c.PaymentTermsJson)
               .HasColumnName("PaymentTermsJson")
               .HasDefaultValueSql("'{}'::jsonb") 
               .HasColumnType("jsonb")
               .IsRequired(false);
        
        // Índices para mejorar el rendimiento
        builder.HasIndex(c => c.ExternalId)
               .HasDatabaseName("IX_Customers_ExternalId")
               .IsUnique();
        
        builder.HasIndex(c => c.Email)
               .HasDatabaseName("IX_Customers_Email");
        
        builder.HasIndex(c => c.TaxId)
               .HasDatabaseName("IX_Customers_TaxId");
        
        // Índices para las propiedades JSON (PostgreSQL específico)
        builder.HasIndex(c => c.SettingsCustomerJson)
               .HasDatabaseName("IX_Customers_SettingsCustomerJson")
               .HasMethod("gin");
        
        builder.HasIndex(c => c.DirectionJson)
               .HasDatabaseName("IX_Customers_DirectionJson")
               .HasMethod("gin");
    }
}