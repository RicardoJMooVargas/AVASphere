﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AVASphere.ApplicationCore.Common.Entities.General;

namespace AVASphere.Infrastructure.Common.Configuration
{
    public class ConfigSysEntitieConfig : IEntityTypeConfiguration<ConfigSys>
    {
        public void Configure(EntityTypeBuilder<ConfigSys> entity)
        {
            entity.ToTable("ConfigSys");
            entity.HasKey(e => e.IdConfigSys);

            entity.Property(e => e.CompanyName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.BranchName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.LogoUrl)
                .HasMaxLength(500);

            // Campos JSONB
            entity.Property(e => e.Colors)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb")
                .IsRequired();

            entity.Property(e => e.NotUseModules)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 🔹 Relación 1-N (ConfigSys → Users)
            entity.HasMany(c => c.Users)
              .WithOne(u => u.ConfigSys)
              .HasForeignKey(u => u.IdConfigSys)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Relación 1-N (ConfigSys → Projects)
            entity.HasMany(c => c.Projects)
              .WithOne(p => p.ConfigSys)
              .HasForeignKey(p => p.IdConfigSys)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Relación 1-N (ConfigSys → Quotations)
            entity.HasMany(c => c.Quotations)
              .WithOne(q => q.ConfigSys)
              .HasForeignKey(q => q.IdConfigSys)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Relación 1-N (ConfigSys → Sales)
            entity.HasMany(c => c.Sales)
              .WithOne(s => s.ConfigSys)
              .HasForeignKey(s => s.IdConfigSys)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Relación 1-N (ConfigSys → ProjectCategories)
            entity.HasMany(c => c.ProjectCategories)
              .WithOne(pc => pc.ConfigSys)
              .HasForeignKey(pc => pc.IdConfigSys)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}