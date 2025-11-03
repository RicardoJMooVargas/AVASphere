using Microsoft.EntityFrameworkCore;
using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Entities.Catalogs;
using AVASphere.ApplicationCore.Common.Entities.General;
using AVASphere.ApplicationCore.Projects.Entities.Catalogs;
using AVASphere.Infrastructure.Common.Configuration;
using AVASphere.Infrastructure.Projects.Configuration;

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
    public DbSet<Customer> Customers { get; set; } = null!;
    //public DbSet<BranchOffice> BranchOffices { get; set; } = null!;
    // MODULO DE SALES
    
    // MODULO PROJECTS
    public DbSet<ProjectCategory> ProjectCategory { get; set; } = null!;
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MODULO DE COMMON
        modelBuilder.ApplyConfiguration(new ConfigSysEntitieConfig());
        modelBuilder.ApplyConfiguration(new UserEntitieConfig());
        modelBuilder.ApplyConfiguration(new RolEntitieConfig());
        modelBuilder.ApplyConfiguration(new AreaEntitieConfig());
        modelBuilder.ApplyConfiguration(new CustomerEntitieConfig());
        //modelBuilder.ApplyConfiguration(new BranchOfficeEntitieConfig());
        // MODULO DE SALES
        
        // MODULO DE PROJECTS
        modelBuilder.ApplyConfiguration(new ProjectCategoryEntitieConfig());
        //........
    }
}