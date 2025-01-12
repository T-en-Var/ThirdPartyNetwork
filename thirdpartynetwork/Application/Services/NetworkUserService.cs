using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkUserService(INetworkUserRepository repository, INetworkService networkService, IUserProxyService userProxyService, IMapper mapper) : INetworkUserService
    {
        public ICollection<NetworkUser> GetAll()
        {
            var networkUsers = repository.GetAll();
            return networkUsers;
        }

        public ICollection<NetworkUser> GetByNetworkId(Guid networkId)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not Found");
            
            var networkUsers = repository.GetByNetworkId(networkId);
            return networkUsers;
        }

        public ICollection<NetworkUser> GetByUserProxyId(Guid userProxyId)
        {
            if (userProxyService.GetById(userProxyId) == null) throw new NotFoundHttpException("UserProxy not Found");
            
            var networkUsers = repository.GetByUserProxyId(userProxyId);
            return networkUsers;
        }

        public NetworkUser? GetById(Guid id)
        {
            var networkUser = repository.GetById(id);
            return networkUser;
        }

        public NetworkUser Create(Guid networkId, CreateNetworkUserDto createNetworkUser)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not Found");
            
            var networkUser = mapper.Map<NetworkUser>(createNetworkUser);
            networkUser.NetworkId = networkId;
            
            networkUser = repository.Create(networkUser);
            return networkUser;
        }

        public NetworkUser Update(Guid networkId, Guid networkUserId, UpdateNetworkUserDto updateNetworkUser)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not Found");
            if (!repository.Exists(networkId)) throw new NotFoundHttpException();
            
            var networkUser = mapper.Map<NetworkUser>(updateNetworkUser);
            networkUser.Id = networkUserId;
            networkUser.NetworkId = networkId;
            
            networkUser = repository.Update(networkUser);
            return networkUser;
        }

        public bool DeleteById(Guid id)
        {
            var networkUser = repository.GetById(id);
            return networkUser != null && repository.Delete(networkUser);
        }
    }
}
