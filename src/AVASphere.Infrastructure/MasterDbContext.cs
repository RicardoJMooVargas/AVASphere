using Microsoft.EntityFrameworkCore;
using AVASphere.ApplicationCore.Common.Entities;

namespace AVASphere.Infrastructure;

public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options)
        : base(options)
    {
    }
    
    // Agrega tu DbSet
    public DbSet<ConfigSys> ConfigSys { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de la tabla ConfigSys
        modelBuilder.Entity<ConfigSys>(entity =>
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
        });
    }
    
}