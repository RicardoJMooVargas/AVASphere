using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AVASphere.Infrastructure;

public class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
{
    public MasterDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MasterDbContext>();
        
        // Usar la misma cadena de conexión que está en appsettings.json
        var connectionString = "Host=191.96.31.105;Port=5432;Database=avaspheredb;Username=adminvyaa;Password=xuWHDstwihFGW14;";
        
        optionsBuilder.UseNpgsql(connectionString);

        return new MasterDbContext(optionsBuilder.Options);
    }
}