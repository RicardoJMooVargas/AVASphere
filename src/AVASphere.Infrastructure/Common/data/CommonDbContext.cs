using Microsoft.EntityFrameworkCore;
using AVASphere.ApplicationCore.Common.Entities;
namespace AVASphere.Infrastructure.Common.Data;

public class CommonDbContext : DbContext
{
    public CommonDbContext(DbContextOptions<CommonDbContext> options)
        : base(options)
    {
    }

    public DbSet<ConfigSys> Configurations => Set<ConfigSys>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConfigSys>(entity =>
        {
            entity.ToTable("config_sys");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.BranchName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.LogoUrl).IsRequired();
            entity.Property(e => e.PrimaryColor).HasMaxLength(7).IsRequired();
            entity.Property(e => e.SecondaryColor).HasMaxLength(7).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}