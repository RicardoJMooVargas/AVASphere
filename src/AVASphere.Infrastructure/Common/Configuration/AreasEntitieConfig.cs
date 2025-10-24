using AVASphere.ApplicationCore.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.ApplicationCore.Common.Configuration;

public class AreasEntitieConfig : IEntityTypeConfiguration<Areas>
{
    public void Configure(EntityTypeBuilder<Areas> entity)
    {
        entity.ToTable("Areas");
        entity.HasKey(e => e.IdAreas);

        entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired(); // Agregar si es requerido

        entity.Property(e => e.NormalizedName)
            .HasMaxLength(100)
            .IsRequired(); // Agregar si es requerido

        // Configurar la relación con Rols si es necesario
        entity.HasMany(a => a.Rols)
            .WithOne() // O .WithOne(r => r.Area) si tienes propiedad de navegación en Rols
            .HasForeignKey("AreaId") // Especificar FK si es necesario
            .OnDelete(DeleteBehavior.Cascade); // O el comportamiento que necesites
    }
}