using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Entities.General;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IConfigSysRepository
{
    Task<ConfigSys?> GetAsync();
    Task AddOrUpdateAsync(ConfigSys config);
}