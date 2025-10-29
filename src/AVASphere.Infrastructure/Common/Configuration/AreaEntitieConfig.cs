using AVASphere.ApplicationCore.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.Infrastructure.Common.Configuration;

public class AreaEntitieConfig : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> entity)
    {
        entity.ToTable("Area");
        entity.HasKey(e => e.IdArea);

        entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.NormalizedName)
            .HasMaxLength(100)
            .IsRequired();

        // SOLO UNA configuración de relación
        entity.HasMany(a => a.Rol)
            .WithOne(r => r.Area)
            .HasForeignKey(r => r.IdArea) // Usar solo IdArea
            .OnDelete(DeleteBehavior.Cascade);
    }
}