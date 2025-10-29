using AVASphere.ApplicationCore.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVASphere.ApplicationCore.Common.Configuration;

public class ConfigSysEntitieConfig : IEntityTypeConfiguration<ConfigSys>
{
    public void Configure(EntityTypeBuilder<ConfigSys> entity)
    {
        entity.ToTable("ConfigSys");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.CompanyName)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(e => e.BranchName)
            .HasMaxLength(200);

        entity.Property(e => e.LogoUrl)
            .HasMaxLength(500);

        entity.Property(e => e.PrimaryColor)
            .HasMaxLength(10);

        entity.Property(e => e.SecondaryColor)
            .HasMaxLength(10);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}