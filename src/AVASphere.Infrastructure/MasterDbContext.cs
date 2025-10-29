using AVASphere.ApplicationCore.Common.Configuration;
using Microsoft.EntityFrameworkCore;
using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.Infrastructure.Common.Configuration;

namespace AVASphere.Infrastructure;

public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options)
        : base(options)
    {
    }
    // MODULO DE COMMON
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Rol> Rols { get; set; } = null!;
    public DbSet<Area> Areas { get; set; } = null!;
    public DbSet<ConfigSys> ConfigSys { get; set; } = null!;
    // MODULO DE SALES
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MODULO DE COMMON
        modelBuilder.ApplyConfiguration(new ConfigSysEntitieConfig());
        modelBuilder.ApplyConfiguration(new UserEntitieConfig());
        modelBuilder.ApplyConfiguration(new RolEntitieConfig());
        modelBuilder.ApplyConfiguration(new AreaEntitieConfig());
        // MODULO DE SALES
        //........
    }
}