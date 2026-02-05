﻿﻿using AVASphere.ApplicationCore.Common.Entities.Catalogs;
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

        // Relación 1-N con StorageStructure
        entity.HasMany(a => a.StorageStructures)
            .WithOne(ss => ss.Area)
            .HasForeignKey(ss => ss.IdArea)
            .OnDelete(DeleteBehavior.SetNull);
    }
}