using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkRoleService(INetworkRoleRepository repository, INetworkService networkService, IRoleService roleService, IMapper mapper) : INetworkRoleService
    {
        public ICollection<NetworkRole> GetAll()
        {
            var networkRoles = repository.GetAll();
            return networkRoles;
        }

        public ICollection<NetworkRole> GetByNetworkId(Guid id)
        {
            if (networkService.GetById(id) == null) throw new NotFoundHttpException("Network not found.");
            
            var networkRoles = repository.GetByNetworkId(id);
            return networkRoles;
        }

        public ICollection<NetworkRole> GetByRoleId(Guid id)
        {
            if (roleService.GetById(id) == null) throw new NotFoundHttpException("Role not found.");
            
            var networkRoles = repository.GetByNetworkId(id);
            return networkRoles;
        }

        public NetworkRole? GetByIds(Guid networkId, Guid roleId)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found.");
            if (roleService.GetById(roleId) == null) throw new NotFoundHttpException("Role not found.");

            var networkRole = repository.GetByIds(networkId, roleId);
            return networkRole;
        }

        public NetworkRole Create(Guid networkId, Guid roleId, CreateNetworkRoleDto createNetworkRole)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found.");
            if (roleService.GetById(roleId) == null) throw new NotFoundHttpException("Role not found.");
            
            if (repository.Exists(networkId, roleId)) throw new ConflictHttpException("NetworkPermission already exists.");
            
            var networkRole = mapper.Map<NetworkRole>(createNetworkRole);
            networkRole.NetworkId = networkId;
            networkRole.RoleId = roleId;
            
            networkRole = repository.Create(networkRole);
            return networkRole;
        }

        public NetworkRole Update(Guid networkId, Guid roleId, UpdateNetworkRoleDto updateNetworkRole)
        {
            if (!repository.Exists(networkId, roleId)) throw new NotFoundHttpException();
            
            var networkRole = mapper.Map<NetworkRole>(updateNetworkRole);
            networkRole.NetworkId = networkId;
            networkRole.RoleId = roleId;
            networkRole = repository.Update(networkRole);
            return networkRole;
        }

        public bool DeleteByIds(Guid networkId, Guid roleId)
        {
            var networkRole = GetByIds(networkId, roleId);
            return networkRole != null && repository.Delete(networkRole);
        }
    }
}
