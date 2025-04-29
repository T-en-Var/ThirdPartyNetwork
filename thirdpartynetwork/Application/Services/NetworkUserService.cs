using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkUserService(
        INetworkUserRepository repository,
        INetworkService networkService,
        IUserProxyService userProxyService,
        IMapper mapper) : INetworkUserService
    {
        public ICollection<NetworkUser> GetAll()
        {
            var networkUsers = repository.GetAll();
            return networkUsers;
        }

        public NetworkUser? GetByIds(Guid networkId, Guid userProxyId)
        {
            var networkUser = repository.GetByIds(networkId, userProxyId);
            return networkUser;
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

        public ICollection<Network> GetNetworksByUserProxyId(Guid userProxyId)
        {
            if (userProxyService.GetById(userProxyId) == null) throw new NotFoundHttpException("UserProxy not Found");

            var networks = repository.GetByUserProxyId(userProxyId)
                .Select(nu => nu.Network)
                .ToList();

            return networks;
        }

        public NetworkUser? GetById(Guid networkId, Guid id)
        {
            var networkUser = repository.GetById(networkId, id);
            return networkUser;
        }

        public NetworkUser? GetByNetworkIdAndUsername(Guid networkId, string username)
        {
            var networkUser = repository.GetByNetworkIdAndUsername(networkId, username);
            return networkUser;
        }

        public NetworkUser Create(Guid networkId, Guid userProxyId, CreateNetworkUserDto createNetworkUser)
        {
            var network = networkService.GetDetailsById(networkId);
            if (network == null) throw new NotFoundHttpException("Network not Found");
            if (userProxyService.GetById(userProxyId) == null) throw new NotFoundHttpException("UserProxy not Found");

            if (GetByIds(networkId, userProxyId) != null) throw new ConflictHttpException("NetworkUser already exists");

            var networkUser = mapper.Map<NetworkUser>(createNetworkUser);
            networkUser.NetworkId = networkId;
            networkUser.UserProxyId = userProxyId;

            foreach (var networkAccess in network.NetworkAccesses)
            {
                networkUser.NetworkUserAccesses.Add(new NetworkUserAccess()
                {
                    Access = networkAccess.Access,
                    IsAccepted = networkAccess.IsRequired,
                    NetworkUser = networkUser
                });
            }

            networkUser = repository.Create(networkUser);
            return networkUser;
        }

        public NetworkUser Update(Guid networkId, Guid networkUserId, UpdateNetworkUserDto updateNetworkUser)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not Found");
            if (!repository.Exists(networkUserId)) throw new NotFoundHttpException();

            var networkUser = mapper.Map<NetworkUser>(updateNetworkUser);
            networkUser.Id = networkUserId;
            networkUser.NetworkId = networkId;

            networkUser = repository.Update(networkId, networkUser);
            return networkUser;
        }

        public bool DeleteById(Guid networkId, Guid id)
        {
            var networkUser = repository.GetById(networkId, id);
            return networkUser != null && repository.Delete(networkUser);
        }
    }
}