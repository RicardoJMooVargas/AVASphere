using AVASphere.ApplicationCore.Common.Entities;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IConfigSysService
{
    Task<ConfigSys?> GetConfigAsync();
    Task<ConfigSys> SaveConfigAsync(ConfigSys config);
}