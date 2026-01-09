using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;
using thirdpartynetwork.Application.Dtos.UserContent.Configuration;

namespace thirdpartynetwork.Application.Services;

public class ConfigurationService(
    IConfigurationRepository repository,
    INetworkService networkService,
    IIdentityService identityService,
    INetworkUserService networkUserService,
    IMapper mapper) : UserContentBaseService<Configuration>(repository, networkUserService), IConfigurationService
{
    private readonly INetworkUserService _networkUserService = networkUserService;

    public async Task<ICollection<Configuration>> GetByNetworkIdAsync(Guid networkId)
    {
        return await repository.GetByNetworkIdAsync(networkId);
    }

    public async Task<Configuration?> GetByIdAsync(Guid networkId, Guid configurationId)
    {
        return await repository.GetByIdAsync(configurationId);
    }

    public async Task<Configuration> CreateAsync(Guid networkId, CreateConfigurationDto createConfiguration)
    {
        var network = await networkService.GetByIdAsync(networkId);
        if (network == null) throw new NotFoundHttpException("Network not found");

        var userProxy = await identityService.GetUserProxyAsync();
        var networkUser = await _networkUserService.GetByIdsAsync(networkId, userProxy.Id);
        if (networkUser == null) throw new NotFoundHttpException("NetworkUser not found");

        var configuration = mapper.Map<Configuration>(createConfiguration);
        configuration.Network = network;
        configuration.Author = networkUser;

        return await repository.CreateAsync(configuration);
    }

    public async Task<Configuration> UpdateAsync(Guid networkId, Guid configurationId, UpdateConfigurationDto updateConfiguration)
    {
        if (!await repository.ExistsAsync(configurationId)) throw new NotFoundHttpException();

        var network = await networkService.GetByIdAsync(networkId);
        if (network == null) throw new NotFoundHttpException("Network not found");

        var configuration = await repository.UpdateAsync(configurationId, updateConfiguration);
        return configuration;
    }

    public async Task DeleteByIdAsync(Guid networkId, Guid configurationId)
    {
        var configuration = await GetByIdAsync(networkId, configurationId);
        if (configuration == null) return;

        await repository.DeleteAsync(configuration);
    }
}
