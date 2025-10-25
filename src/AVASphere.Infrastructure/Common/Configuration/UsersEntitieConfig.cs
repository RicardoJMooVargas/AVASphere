using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AVASphere.ApplicationCore.Common.Entities;

namespace AVASphere.Infrastructure.Common.Configuration
{
    public class UsersEntitieConfig : IEntityTypeConfiguration<Users>
    {
        public void Configure(EntityTypeBuilder<Users> entity)
        {
            entity.ToTable("Users");
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

            entity.HasOne(u => u.Rols)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.IdRols)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}