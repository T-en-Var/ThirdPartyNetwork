using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkPermissionService(INetworkPermissionRepository repository, INetworkService networkService, IPermissionService permissionService, IMapper mapper) : INetworkPermissionService
    {
        public ICollection<NetworkPermission> GetAll()
        {
            var networkPermissions = repository.GetAll();
            return networkPermissions;
        }

        public ICollection<NetworkPermission> GetByNetworkId(Guid id)
        {
            if (networkService.GetById(id) == null) throw new NotFoundHttpException("Network not found.");
            
            var networkPermissions = repository.GetByNetworkId(id);
            return networkPermissions;
        }

        public ICollection<NetworkPermission> GetByPermissionId(Guid id)
        {
            if (permissionService.GetById(id) == null) throw new NotFoundHttpException("Permission not found.");
            
            var networkPermissions = repository.GetByPermissionId(id);
            return networkPermissions;
        }

        public NetworkPermission? GetByIds(Guid networkId, Guid permissionId)
        {
            var networkPermission = repository.GetByIds(networkId, permissionId);
            return networkPermission;
        }

        public NetworkPermission Create(Guid networkId, Guid permissionId, CreateNetworkPermissionDto createNetworkPermission)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found.");
            if (permissionService.GetById(permissionId) == null) throw new NotFoundHttpException("Permission not found.");
            
            if (repository.Exists(networkId, permissionId)) throw new ConflictHttpException("NetworkPermission already exists.");
            
            var networkPermission = mapper.Map<NetworkPermission>(createNetworkPermission);
            
            networkPermission.NetworkId = networkId;
            networkPermission.PermissionId = permissionId;
            
            networkPermission = repository.Create(networkPermission);
            
            return networkPermission;
        }

        public NetworkPermission Update(Guid networkId, Guid permissionId, UpdateNetworkPermissionDto updateNetworkPermission)
        {
            if (!repository.Exists(networkId, permissionId)) throw new NotFoundHttpException();
            
            var networkPermission = mapper.Map<NetworkPermission>(updateNetworkPermission);
            networkPermission.NetworkId = networkId;
            networkPermission.PermissionId = permissionId;
            
            networkPermission = repository.Update(networkPermission);
            return networkPermission;
        }

        public bool DeleteByIds(Guid networkId, Guid permissiodId)
        {
            var networkPermission = repository.GetByIds(networkId, permissiodId);
            return networkPermission != null && repository.Delete(networkPermission);
        }
    }
}
