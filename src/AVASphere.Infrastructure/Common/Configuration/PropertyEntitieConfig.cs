//ACTUALIZADO A LA VERSION 0.2 DE LA DB
using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Common.Configuration;

public class PropertyEntitieConfig : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> entity)
    {
        entity.ToTable("Property");
        entity.HasKey(e => e.IdProperty);

        entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.NormalizedName)
            .HasMaxLength(100)
            .IsRequired();

        // Configuración de relación
        entity.HasMany(p => p.CatalogValue)
            .WithOne(pv => pv.Property)
            .HasForeignKey(pv => pv.IdProperty)
            .OnDelete(DeleteBehavior.Cascade);
    }
}