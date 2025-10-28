using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AVASphere.ApplicationCore.Common.Entities;

namespace AVASphere.Infrastructure.Common.Configuration
{
    public class UserEntitieConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.ToTable("User");
            entity.HasKey(e => e.IdUsers);
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .IsRequired();
            entity.HasIndex(e => e.UserName)
                .IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.HashPassword).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Aux).HasMaxLength(100);
            entity.Property(e => e.CreateAt).HasMaxLength(50);
            entity.Property(e => e.Verified).HasMaxLength(10);

            entity.HasOne(u => u.Rol)
                .WithMany(r => r.User)
                .HasForeignKey(u => u.IdRol)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}