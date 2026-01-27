//ACTUALIZADO A LA VERSION 0.2 DE LA DB
using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Common.Configuration;

public class PropertyValueEntitieConfig : IEntityTypeConfiguration<PropertyValue>
{
    public void Configure(EntityTypeBuilder<PropertyValue> entity)
    {
        entity.ToTable("PropertyValue");
        entity.HasKey(e => e.IdPropertyValue);

        entity.Property(e => e.Value)
            .HasMaxLength(200);
        
        entity.ToTable("PropertyValue");
        entity.HasKey(e => e.FatherValue);
        
        entity.Property(e => e.Type)
            .HasMaxLength(200);

        entity.Property(e => e.IdProperty)
            .IsRequired();

        // Configuración de relaciones
        entity.HasMany(pv => pv.ProductProperties)
            .WithOne(pp => pp.PropertyValue)
            .HasForeignKey(pp => pp.IdPropertyValue)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(pv => pv.IndividualListingProperties)
            .WithOne(ilp => ilp.ProductValue)
            .HasForeignKey(ilp => ilp.IdPropertyValue)
            .OnDelete(DeleteBehavior.Cascade);
    }
}