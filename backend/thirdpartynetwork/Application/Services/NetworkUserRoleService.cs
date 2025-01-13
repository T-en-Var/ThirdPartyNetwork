using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkUserRoleService(INetworkUserRoleRepository repository, INetworkService networkService, INetworkUserService networkUserService, IRoleService roleService, IMapper mapper) : INetworkUserRoleService
    {
        public ICollection<NetworkUserRole> GetAll()
        {
            var networkUserRoles = repository.GetAll();
            return networkUserRoles;
        }

        public ICollection<NetworkUserRole> GetByNetworkUserId(Guid networkId, Guid networkUserId)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (networkUserService.GetById(networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            
            var networkUserRoles = repository.GetByNetworkUserId(networkUserId);
            return networkUserRoles;
        }

        public ICollection<NetworkUserRole> GetByRoleId(Guid roleId)
        {
            if (roleService.GetById(roleId) == null) throw new NotFoundHttpException("Role not found");
            
            var networkUserRoles = repository.GetByRoleId(roleId);
            return networkUserRoles;
        }

        public NetworkUserRole? GetByIds(Guid networkId, Guid networkUserId, Guid roleId)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (networkUserService.GetById(networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            if (roleService.GetById(roleId) == null) throw new NotFoundHttpException("Role not found");

            var networkUserRole = repository.GetByIds(networkUserId, roleId);
            return networkUserRole;
        }

        public NetworkUserRole Create(Guid networkId, Guid networkUserId, Guid roleId, CreateNetworkUserRoleDto createNetworkUserRole)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (networkUserService.GetById(networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            if (roleService.GetById(roleId) == null) throw new NotFoundHttpException("Role not found");
                
            if (repository.Exists(networkUserId, roleId)) throw new ConflictHttpException();
            
            var networkUserRole = mapper.Map<NetworkUserRole>(createNetworkUserRole);
            networkUserRole.NetworkUserId = networkUserId;
            networkUserRole.RoleId = roleId;
            
            networkUserRole = repository.Create(networkUserRole);
            return networkUserRole;
        }

        public NetworkUserRole Update(Guid networkId, Guid networkUserId, Guid roleId, UpdateNetworkUserRoleDto updateNetworkUserRole)
        {
            if (!repository.Exists(networkUserId, roleId)) throw new NotFoundHttpException();
            
            var networkUserRole = mapper.Map<NetworkUserRole>(updateNetworkUserRole);
            networkUserRole.NetworkUserId = networkUserId;
            networkUserRole.RoleId = roleId;
            
            networkUserRole = repository.Update(networkUserRole);
            return networkUserRole;
        }

        public bool DeleteByIds(Guid networkId, Guid networkUserId, Guid roleId)
        {
            var networkUserRole = repository.GetByIds(networkId, roleId);
            return networkUserRole != null && repository.Delete(networkUserRole);
        }
    }
}
