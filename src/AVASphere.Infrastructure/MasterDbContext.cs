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
    public DbSet<Users> Users { get; set; } = null!;
    public DbSet<Rols> Rols { get; set; } = null!;
    public DbSet<Areas> Areas { get; set; } = null!;
    public DbSet<ConfigSys> ConfigSys { get; set; } = null!;
    // MODULO DE SALES
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MODULO DE COMMON
        modelBuilder.ApplyConfiguration(new ConfigSysEntitieConfig());
        modelBuilder.ApplyConfiguration(new UsersEntitieConfig());
        modelBuilder.ApplyConfiguration(new RolsEntitieConfig());
        modelBuilder.ApplyConfiguration(new AreasEntitieConfig());
        // MODULO DE SALES
        //........
    }
}