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
    IMapper mapper) : IConfigurationService
{
    public ICollection<Configuration> GetByNetworkId(Guid networkId)
    {
        return repository.GetByNetworkId(networkId);
    }

    public Configuration? GetById(Guid networkId, Guid configurationId)
    {
        return repository.GetById(configurationId);
    }

    public Configuration Create(Guid networkId, CreateConfigurationDto createConfiguration)
    {
        var network = networkService.GetById(networkId);
        if (network == null) throw new NotFoundHttpException("Network not found");

        var userProxy = identityService.GetUserProxy();
        var networkUser = networkUserService.GetByIds(networkId, userProxy.Id);
        if (networkUser == null) throw new NotFoundHttpException("NetworkUser not found");

        var configuration = mapper.Map<Configuration>(createConfiguration);
        configuration.Network = network;
        configuration.Author = networkUser;

        return repository.Create(configuration);
    }

    public Configuration Update(Guid networkId, Guid configurationId, UpdateConfigurationDto updateConfiguration)
    {
        if (!repository.Exists(configurationId)) throw new NotFoundHttpException();

        var network = networkService.GetById(networkId);
        if (network == null) throw new NotFoundHttpException("Network not found");

        var configuration = repository.Update(configurationId, updateConfiguration);
        return configuration;
    }

    public void DeleteById(Guid networkId, Guid configurationId)
    {
        var configuration = GetById(networkId, configurationId);
        if (configuration == null) return;

        repository.Delete(configuration);
    }
}
