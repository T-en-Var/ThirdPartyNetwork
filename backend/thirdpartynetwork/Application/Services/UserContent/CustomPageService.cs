using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class CustomPageService(
        ICustomPageRepository repository,
        INetworkService networkService,
        IIdentityService identityService,
        INetworkUserService networkUserService,
        IFeatureFlagValidator featureFlagService,
        IMapper mapper
    ) : UserContentBaseService<CustomPage>(repository, networkUserService), ICustomPageService
    {
        private readonly INetworkUserService _networkUserService = networkUserService;

        public async Task<ICollection<CustomPage>> GetByNetworkIdAsync(Guid networkId)
        {
            var customPage = await repository.GetByNetworkIdAsync(networkId);
            return customPage;
        }

        public async Task<CustomPage?> GetByIdAsync(Guid networkId, Guid customPageId)
        {
            var customPage = await repository.GetByIdAsync(customPageId);
            return customPage;
        }

        public async Task<CustomPage> CreateAsync(Guid networkId, CreateCustomPageDto createCustomPage)
        {
            var network = await networkService.GetByIdAsync(networkId);
            if (network == null) throw new NotFoundHttpException("Network not found");

            var userProxy = await identityService.GetUserProxyAsync();
            var networkUser = await _networkUserService.GetByIdsAsync(networkId, userProxy.Id);
            if (networkUser == null) throw new NotFoundHttpException("NetworkUser not found");

            if (!await featureFlagService.CheckUserAddCustomPageFeatureFlagAsync(networkUser.UserProxyId, networkId))
                throw new UnauthorizedHttpException("You are exceeding a custom page limit.");

            var customPage = mapper.Map<CustomPage>(createCustomPage);
            customPage.Network = network;
            customPage.AuthorUserProxyId = networkUser.UserProxyId;

            customPage = await repository.CreateAsync(customPage);
            return customPage;
        }

        public async Task<CustomPage> UpdateAsync(Guid networkId, Guid customPageId,
            UpdateCustomPageDto updateCustomPage)
        {
            if (!await repository.ExistsAsync(customPageId)) throw new NotFoundHttpException();

            var network = await networkService.GetByIdAsync(networkId);
            if (network == null) throw new NotFoundHttpException("Network not found");

            var customPage = await repository.UpdateAsync(networkId, customPageId, updateCustomPage);
            return customPage;
        }

        public async Task DeleteByIdAsync(Guid networkId, Guid customPageId)
        {
            var customPage = await GetByIdAsync(networkId, customPageId);
            if (customPage != null) await repository.DeleteAsync(customPage);
        }
    }
}