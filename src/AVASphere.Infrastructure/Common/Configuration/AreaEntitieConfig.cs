using AVASphere.ApplicationCore.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.ApplicationCore.Common.Configuration;

public class AreaEntitieConfig : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> entity)
    {
        entity.ToTable("Area");
        entity.HasKey(e => e.IdArea);

        entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired(); // Agregar si es requerido

        entity.Property(e => e.NormalizedName)
            .HasMaxLength(100)
            .IsRequired(); // Agregar si es requerido

        // Configurar la relación con Rol si es necesario
        entity.HasMany(a => a.Rol)
            .WithOne() // O .WithOne(r => r.Area) si tienes propiedad de navegación en Rol
            .HasForeignKey("AreaId") // Especificar FK si es necesario
            .OnDelete(DeleteBehavior.Cascade); // O el comportamiento que necesites
    }
}