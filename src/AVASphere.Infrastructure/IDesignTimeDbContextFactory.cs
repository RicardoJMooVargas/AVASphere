using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AVASphere.Infrastructure;

public class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
{
    public MasterDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MasterDbContext>();
        optionsBuilder.UseNpgsql("Host=191.96.31.105;Port=5432;Database=avaspheredb;Username=adminvyaa;Password=xuWHDstwihFGW14;");

        return new MasterDbContext(optionsBuilder.Options);
    }
}