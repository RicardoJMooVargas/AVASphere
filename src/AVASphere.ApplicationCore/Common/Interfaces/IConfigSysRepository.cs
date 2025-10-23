using AVASphere.ApplicationCore.Common.Entities;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IConfigSysRepository
{
    Task<ConfigSys?> GetAsync();
    Task AddOrUpdateAsync(ConfigSys config);
}