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
        entity.HasKey(e => e.IdPropertyValue); // La llave primaria correcta

        entity.Property(e => e.Value)
            .HasMaxLength(200);
        
        entity.Property(e => e.Type)
            .HasMaxLength(200);

        entity.Property(e => e.IdProperty)
            .IsRequired();

        // FatherValue es nullable para permitir jerarquías (padre puede ser null)
        entity.Property(e => e.FatherValue)
            .IsRequired(false); // Permite null

        // Relación jerárquica: PropertyValue puede tener PropertyValues hijos
        entity.HasMany<PropertyValue>()
            .WithOne()
            .HasForeignKey(pv => pv.FatherValue)
            .OnDelete(DeleteBehavior.Restrict); // Evita eliminar padre si tiene hijos

        // Configuración de relaciones
        entity.HasMany(pv => pv.ProductProperties)
            .WithOne(pp => pp.PropertyValue)
            .HasForeignKey(pp => pp.IdPropertyValue)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Property
        entity.HasOne(pv => pv.Property)
            .WithMany(p => p.CatalogValue)
            .HasForeignKey(pv => pv.IdProperty)
            .OnDelete(DeleteBehavior.Cascade);
    }
}