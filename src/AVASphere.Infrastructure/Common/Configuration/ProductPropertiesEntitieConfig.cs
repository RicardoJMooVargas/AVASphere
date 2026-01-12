using AVASphere.ApplicationCore.Common.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Common.Configuration;

public class ProductPropertiesEntitieConfig : IEntityTypeConfiguration<ProductProperties>
{
    public void Configure(EntityTypeBuilder<ProductProperties> entity)
    {
        entity.ToTable("ProductProperties");
        entity.HasKey(e => e.IdProductProperties);

        entity.Property(e => e.CustomValue)
            .HasMaxLength(200);

        entity.Property(e => e.IdProduct)
            .IsRequired();

        entity.Property(e => e.IdPropertyValue)
            .IsRequired();

        // La relación con Product ya está configurada en ProductEntitieConfig
        // La relación con PropertyValue ya está configurada en PropertyValueEntitieConfig
    }
}

