using AVASphere.ApplicationCore.Common.Interfaces;
using AVASphere.ApplicationCore.Common.Entities;
namespace AVASphere.Infrastructure.Common.Services;

public class ConfigSysService : IConfigSysService
{
    private readonly IConfigSysRepository _repository;

    public ConfigSysService(IConfigSysRepository repository)
    {
        _repository = repository;
    }

    public async Task<ConfigSys?> GetConfigAsync()
    {
        return await _repository.GetAsync();
    }

    public async Task<ConfigSys> SaveConfigAsync(ConfigSys config)
    {
        await _repository.AddOrUpdateAsync(config);
        return config;
    }
}