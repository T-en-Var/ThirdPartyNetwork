using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class RoleService(IRoleRepository repository, INetworkService networkService, IMapper mapper) : IRoleService
    {
        public ICollection<Role> GetAll()
        {
            var roles = repository.GetAll();
            return roles;
        }
        public ICollection<Role> GetByNetworkId(Guid networkId)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found");
            
            var roles = repository.GetByNetworkId(networkId);
            return roles;
        }

        public Role? GetById(Guid networkId, Guid roleId)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found");
            
            var role = repository.GetById(roleId);
            return role;
        }

        public Role Create(Guid networkId, CreateRoleDto createRole)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found");
            
            var role = mapper.Map<Role>(createRole);
            role.NetworkId = networkId;
            
            role = repository.Create(role);
            return role;
        }

        public Role Update(Guid networkId, Guid roleId, UpdateRoleDto updateRole)
        {
            if (networkService.GetById(networkId) == null) throw new NotFoundHttpException("Network not found");
            
            if (!repository.Exists(roleId)) throw new NotFoundHttpException();
            
            var role = mapper.Map<Role>(updateRole);
            role.Id = roleId;
            
            role = repository.Update(role);
            return role;
        }

        public bool DeleteById(Guid networkId, Guid roleId)
        {
            var role = GetById(networkId, roleId);
            return role != null && repository.Delete(role);
        }
    }
}
