using AVASphere.ApplicationCore.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace AVASphere.Infrastructure.Common.Configuration;

public class RolEntitieConfig : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> entity)
    {
        // PONER EN PLURAL LA TABLA
        entity.ToTable("Rol");
        entity.HasKey(e => e.IdRol);

        entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        entity.Property(e => e.NormalizedName).HasMaxLength(100);

        entity.HasOne(r => r.Area)
            .WithMany(a => a.Rol)
            .HasForeignKey(r => r.IdArea)
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