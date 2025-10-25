using AVASphere.ApplicationCore.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace AVASphere.Infrastructure.Common.Configuration;

public class RolsEntitieConfig : IEntityTypeConfiguration<Rols>
{
    public void Configure(EntityTypeBuilder<Rols> entity)
    {
        entity.ToTable("Rols");
        entity.HasKey(e => e.IdRols);

        entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        entity.Property(e => e.NormalizedName).HasMaxLength(100);

        entity.HasOne(r => r.Areas)
            .WithMany(a => a.Rols)
            .HasForeignKey(r => r.IdAreas)
            .OnDelete(DeleteBehavior.Cascade);

        // Campos JSONB
        entity.Property(e => e.Permissions)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        entity.Property(e => e.Modules)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");
    }
}