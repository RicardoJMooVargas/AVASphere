using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AVASphere.ApplicationCore.Common.Entities.General;

namespace AVASphere.Infrastructure.Common.Configuration
{
    public class UserEntitieConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.ToTable("User");

            entity.HasKey(e => e.IdUser);

            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(e => e.UserName).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.HashPassword).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Aux).HasMaxLength(100);
            entity.Property(e => e.CreateAt).HasMaxLength(50);
            entity.Property(e => e.Verified).HasMaxLength(10);

            // 🔹 Relación con Rol
            entity.HasOne(u => u.Rol)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.IdRol)
                .OnDelete(DeleteBehavior.Cascade); // elimina usuarios si se elimina el rol

            // 🔹 Relación con ConfigSys
            entity.HasOne(u => u.ConfigSys)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.IdConfigSys)
                .OnDelete(DeleteBehavior.Restrict); // evita borrado en cascada del sistema
        }
    }
}