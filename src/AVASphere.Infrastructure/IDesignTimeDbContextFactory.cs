﻿﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AVASphere.Infrastructure;

public class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
{
    public MasterDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MasterDbContext>();
        
        // Usar la cadena de conexión de la base de datos de testing
        var connectionString = "Host=191.96.31.105;Port=5432;Database=avaspheredbtest;Username=adminvyaa;Password=xuWHDstwihFGW14;";
        
        optionsBuilder.UseNpgsql(connectionString);

        return new MasterDbContext(optionsBuilder.Options);
    }
}