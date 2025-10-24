using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace AVASphere.Infrastructure.Common.Repositories;

public class ConfigSysRepository : IConfigSysRepository
{
    private readonly MasterDbContext _context;

    public ConfigSysRepository(MasterDbContext context)
    {
        _context = context;
    }

    public async Task<ConfigSys?> GetAsync()
    {
        return await _context.ConfigSys.FirstOrDefaultAsync();
    }

    public async Task AddOrUpdateAsync(ConfigSys config)
    {
        var existing = await _context.ConfigSys.FirstOrDefaultAsync();
        if (existing == null)
        {
            await _context.ConfigSys.AddAsync(config);
        }
        else
        {
            existing.CompanyName = config.CompanyName;
            existing.BranchName = config.BranchName;
            existing.LogoUrl = config.LogoUrl;
            existing.PrimaryColor = config.PrimaryColor;
            existing.SecondaryColor = config.SecondaryColor;
        }

        await _context.SaveChangesAsync();
    }
}