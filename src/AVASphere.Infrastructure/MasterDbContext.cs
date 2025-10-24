using AVASphere.ApplicationCore.Common.Configuration;
using Microsoft.EntityFrameworkCore;
using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.Infrastructure.Common.Configuration;

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
        // MODULE Common
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConfigSysEntitieConfig).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersEntitieConfig).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RolsEntitieConfig).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AreasEntitieConfig).Assembly);
    }
}
