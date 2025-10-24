using Microsoft.EntityFrameworkCore;
using AVASphere.ApplicationCore.Common.Entities;

namespace AVASphere.Infrastructure;

public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options)
        : base(options) { }

    public DbSet<Users> Users { get; set; } = null!;
    public DbSet<Rols> Rols { get; set; } = null!;
    public DbSet<Areas> Areas { get; set; } = null!;
    public DbSet<ConfigSys> ConfigSys { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // === USERS ===
        modelBuilder.Entity<Users>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.IdUsers);

            entity.Property(e => e.UserName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.HashPassword).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Aux).HasMaxLength(100);
            entity.Property(e => e.CreateAt).HasMaxLength(50);
            entity.Property(e => e.Verified).HasMaxLength(10);

            // Relación FK → Rol
            entity.HasOne(u => u.Rols)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.IdRols)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // === ROLS ===
        modelBuilder.Entity<Rols>(entity =>
        {
            entity.ToTable("Rols");
            entity.HasKey(e => e.IdRols);

            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.NormalizedName).HasMaxLength(100);

            // Relación FK → Área
            entity.HasOne(r => r.Areas)
                  .WithMany(a => a.Rols)
                  .HasForeignKey(r => r.IdAreas)
                  .OnDelete(DeleteBehavior.Cascade);

            // === Campos JSONB ===
            entity.Property(e => e.Permissions)
                  .HasColumnType("jsonb")
                  .HasDefaultValueSql("'[]'::jsonb");

            entity.Property(e => e.Modules)
                  .HasColumnType("jsonb")
                  .HasDefaultValueSql("'[]'::jsonb");
        });

        // === AREAS ===
        modelBuilder.Entity<Areas>(entity =>
        {
            entity.ToTable("Areas");
            entity.HasKey(e => e.IdAreas);

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.NormalizedName).HasMaxLength(100);
        });

        // === CONFIGSYS (ya la tenías) ===
        modelBuilder.Entity<ConfigSys>(entity =>
        {
            entity.ToTable("ConfigSys");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.BranchName).HasMaxLength(200);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.PrimaryColor).HasMaxLength(10);
            entity.Property(e => e.SecondaryColor).HasMaxLength(10);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
