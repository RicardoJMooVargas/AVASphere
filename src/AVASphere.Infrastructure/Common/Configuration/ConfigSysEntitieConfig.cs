using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Sales.Entities;

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
                .HasMaxLength(200);

            entity.Property(e => e.LogoUrl)
                .HasMaxLength(500);

            // Campos JSONB
            entity.Property(e => e.Colors)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb");

            entity.Property(e => e.NotUseModules)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'[]'::jsonb");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 🔹 Relación 1-N (ConfigSys → Users)
            entity.HasMany(c => c.Users)
              .WithOne(u => u.ConfigSys)
              .HasForeignKey(u => u.IdConfigSys)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Relación 1-N (ConfigSys → Quotations)
            entity.HasMany(q => q.Quotations)
              .WithOne(q => q.ConfigSys)
              .HasForeignKey(q => q.QuotationId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}