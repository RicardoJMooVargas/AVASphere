using AVASphere.ApplicationCore.Common.Entities;
using AVASphere.ApplicationCore.Common.Entities.General;

namespace AVASphere.ApplicationCore.Common.Interfaces;

public interface IConfigSysService
{
    Task<ConfigSys?> GetConfigAsync();
    Task<ConfigSys> SaveConfigAsync(ConfigSys config);
}