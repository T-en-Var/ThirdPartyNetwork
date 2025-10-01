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

        public NetworkUser? GetByIdsPreAuth(Guid networkId, Guid userProxyId)
        {
            var networkUser = repository.GetByIdsPreAuth(networkId, userProxyId);
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

        public NetworkUser? GetByNetworkIdAndEmail(Guid networkId, string email)
        {
            var networkUser = repository.GetByNetworkIdAndEmail(networkId, email);
            return networkUser;
        }

        public NetworkUser Create(Guid networkId, Guid userProxyId, CreateNetworkUserDto createNetworkUser)
        {
            var network = networkService.GetById(networkId, bypass: true);
            if (network == null) throw new NotFoundHttpException("Network not Found");
            
            var userProxy = userProxyService.GetById(userProxyId);
            if (userProxy == null) throw new NotFoundHttpException("UserProxy not Found");

            var isNotDefaultFlag = userProxy.Email == null;
            userProxy = userProxy.ApplyDefaults();
            
            if (GetByIds(networkId, userProxyId) != null) throw new ConflictHttpException("Email is already in use.");
            if (GetByNetworkIdAndEmail(networkId, userProxy.Email) != null) throw new ConflictHttpException("User with this email already exists" +
                (isNotDefaultFlag ? ". (You email was imported from your default proxy)" : ""));

            var networkUser = mapper.Map<NetworkUser>(createNetworkUser);
            networkUser.NetworkId = networkId;
            networkUser.UserProxyId = userProxyId;
            
            foreach (var networkAccess in network.NetworkAccesses)
            {
                networkUser.NetworkUserAccesses.Add(new NetworkUserAccess()
                {
                    AccessId = networkAccess.AccessId,
                    IsAccepted = networkAccess.IsRequired,
                    NetworkUserId = networkUser.Id,
                    Access = default!,
                    NetworkUser = default!,
                });
            }
            
            foreach (var role in network.Roles)
            {
                if (!role.IsDefault) continue;
                
                networkUser.NetworkUserRoles.Add(new NetworkUserRole()
                {
                    RoleId = role.Id,
                    NetworkUserId = networkUser.Id,
                    Role = default!,
                    NetworkUser = default!,
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

        public void NetworkAccessUpdate(Guid networkId)
        {
            repository.NetworkAccessUpdate(networkId);
        }

        public void DeleteById(Guid networkId, Guid id)
        {
            var network = networkService.GetById(networkId);
            if (network is { IsSystemProtected: true }) throw new UnauthorizedHttpException("Network is system protected");

            var networkUser = repository.GetById(networkId, id);
            if (networkUser != null) repository.Delete(networkUser);
        }
    }
}