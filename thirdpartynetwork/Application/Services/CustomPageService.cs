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
        IMapper mapper) : ICustomPageService
    {
        public ICollection<CustomPage> GetAll()
        {
            var customPage = repository.GetAll();
            return customPage;
        }

        public ICollection<CustomPage> GetByNetworkId(Guid networkId)
        {
            var customPage = repository.GetByNetworkId(networkId);
            return customPage;
        }

        public CustomPage? GetById(Guid networkId, Guid customPageId)
        {
            var customPage = repository.GetById(customPageId);
            return customPage;
        }

        public CustomPage Create(Guid networkId, CreateCustomPageDto createCustomPage)
        {
            var network = networkService.GetById(networkId);
            if (network == null) throw new NotFoundHttpException("Network not found");

            var userProxy = identityService.GetUserProxy();
            var networkUser = networkUserService.GetByIds(networkId, userProxy.Id);
            if (networkUser == null) throw new NotFoundHttpException("NetworkUser not found");

            var customPage = mapper.Map<CustomPage>(createCustomPage);
            customPage.Network = network;
            customPage.Author = networkUser;

            customPage = repository.Create(customPage);
            return customPage;
        }

        public CustomPage Update(Guid networkId, Guid customPageId, UpdateCustomPageDto updateCustomPage)
        {
            if (!repository.Exists(customPageId)) throw new NotFoundHttpException();

            var network = networkService.GetById(networkId);
            if (network == null) throw new NotFoundHttpException("Network not found");

            var customPage = repository.Update(customPageId, updateCustomPage);
            return customPage;
        }

        public void DeleteById(Guid networkId, Guid customPageId)
        {
            var customPage = GetById(networkId, customPageId);
            if (customPage != null) repository.Delete(customPage);
        }
    }
}