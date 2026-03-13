﻿using AVASphere.ApplicationCore.Common.Entities.Catalogs;
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

        // RELACIONES
        entity.HasMany(a => a.Rol)
            .WithOne(r => r.Area)
            .HasForeignKey(r => r.IdArea)
            .OnDelete(DeleteBehavior.Cascade);

        // Las relaciones con StorageStructure y LocationDetails se configuran
        // desde sus respectivas configuraciones para evitar configuraciones duplicadas
    }
}