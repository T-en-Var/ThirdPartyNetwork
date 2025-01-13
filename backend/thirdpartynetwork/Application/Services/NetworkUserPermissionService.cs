using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkUserPermissionService(INetworkUserPermissionRepository repository, INetworkService networkService, INetworkUserService networkUserService, IPermissionService permissionService, IMapper mapper) : INetworkUserPermissionService
    {
        public ICollection<NetworkUserPermission> GetAll()
        {
            var networkUserPermissions = repository.GetAll();
            return networkUserPermissions;
        }

        public ICollection<NetworkUserPermission> GetByNetworkId(Guid networkId, Guid networkUserId)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (networkUserService.GetById(networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            
            var networkUserPermissions = repository.GetByNetworkUserId(networkUserId);
            return networkUserPermissions;
        }

        public ICollection<NetworkUserPermission> GetByPermissionId(Guid permissionId)
        {
            if (permissionService.GetById(permissionId) == null) throw new NotFoundHttpException("Permission not found");
            
            var networkUserPermissions = repository.GetByPermissionId(permissionId);
            return networkUserPermissions;
        }

        public NetworkUserPermission? GetByIds(Guid networkId, Guid networkUserId, Guid permissionId)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (networkUserService.GetById(networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            if (permissionService.GetById(permissionId) == null) throw new NotFoundHttpException("Permission not found");
            
            var networkUserPermission = repository.GetByIds(networkUserId, permissionId);
            return networkUserPermission;
        }

        public NetworkUserPermission Create(Guid networkId, Guid networkUserId, Guid permissionId, CreateNetworkUserPermissionDto createNetworkUserPermission)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (networkUserService.GetById(networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            if (permissionService.GetById(permissionId) == null) throw new NotFoundHttpException("Permission not found");
            
            if (repository.Exists(networkUserId, permissionId)) throw new ConflictHttpException();
            
            var networkUserPermission = mapper.Map<NetworkUserPermission>(createNetworkUserPermission);
            networkUserPermission.NetworkUserId = networkUserId;
            networkUserPermission.PermissionId = permissionId;
            
            networkUserPermission = repository.Create(networkUserPermission);
            return networkUserPermission;
        }

        public NetworkUserPermission Update(Guid networkId, Guid networkUserId, Guid permissionId, UpdateNetworkUserPermissionDto updateNetworkUserPermission)
        {
            if (!repository.Exists(networkUserId, permissionId)) throw new NotFoundHttpException();

            var networkUserPermission = mapper.Map<NetworkUserPermission>(updateNetworkUserPermission);
            networkUserPermission.NetworkUserId = networkUserId;
            networkUserPermission.PermissionId = permissionId;
            
            networkUserPermission = repository.Update(networkUserPermission);
            return networkUserPermission;
        }

        public bool DeleteByIds(Guid networkId, Guid networkUserId, Guid permissionId)
        {
            var networkUserPermission = GetByIds(networkId, networkUserId, permissionId);
            return networkUserPermission != null && repository.Delete(networkUserPermission);
        }
    }
}
