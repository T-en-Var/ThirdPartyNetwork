using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkUserAccessService(
        INetworkUserAccessRepository repository, 
        INetworkService networkService, 
        INetworkUserService networkUserService, 
        IAccessService accessService, 
        IMapper mapper) : INetworkUserAccessService
    {
        public ICollection<NetworkUserAccess> GetAll()
        {
            var networkUserAccesses = repository.GetAll();
            return networkUserAccesses;
        }

        public ICollection<NetworkUserAccess> GetByNetworkId(Guid networkId, Guid networkUserId)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (networkUserService.GetById(networkId, networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            
            var networkUserAccesses = repository.GetByNetworkUserId(networkUserId);
            return networkUserAccesses;
        }

        public ICollection<NetworkUserAccess> GetByAccessId(Guid accessId)
        {
            if (accessService.GetById(accessId) == null) throw new NotFoundHttpException("Access not found");
            
            var networkUserAccesses = repository.GetByAccessId(accessId);
            return networkUserAccesses;
        }

        public NetworkUserAccess? GetByIds(Guid networkId, Guid networkUserId, Guid accessId)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (networkUserService.GetById(networkId, networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            if (accessService.GetById(accessId) == null) throw new NotFoundHttpException("Access not found");
            
            var networkUserPermission = repository.GetByIds(networkUserId, accessId);
            return networkUserPermission;
        }

        public NetworkUserAccess Create(Guid networkId, Guid networkUserId, Guid permissionId, CreateNetworkUserAccessDto createNetworkUserPermission)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (networkUserService.GetById(networkId, networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            if (accessService.GetById(permissionId) == null) throw new NotFoundHttpException("Access not found");
            
            if (repository.Exists(networkUserId, permissionId)) throw new ConflictHttpException();
            
            var networkUserAccess = mapper.Map<NetworkUserAccess>(createNetworkUserPermission);
            networkUserAccess.NetworkUserId = networkUserId;
            networkUserAccess.AccessId = permissionId;
            
            networkUserAccess = repository.Create(networkUserAccess);
            return networkUserAccess;
        }

        public NetworkUserAccess Update(Guid networkId, Guid networkUserId, Guid permissionId, UpdateNetworkUserAccessDto updateNetworkUserPermission)
        {
            if (!repository.Exists(networkUserId, permissionId)) throw new NotFoundHttpException();

            var networkUserAccess = mapper.Map<NetworkUserAccess>(updateNetworkUserPermission);
            networkUserAccess.NetworkUserId = networkUserId;
            networkUserAccess.AccessId = permissionId;
            
            networkUserAccess = repository.Update(networkUserAccess);
            return networkUserAccess;
        }

        public bool DeleteByIds(Guid networkId, Guid networkUserId, Guid permissionId)
        {
            var networkUserPermission = GetByIds(networkId, networkUserId, permissionId);
            return networkUserPermission != null && repository.Delete(networkUserPermission);
        }
    }
}
