using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class RoleService(IRoleRepository repository, INetworkService networkService, IMapper mapper) : IRoleService
    {
        public async Task<ICollection<Role>> GetAllAsync()
        {
            var roles = await repository.GetAllAsync();
            return roles;
        }
        public async Task<ICollection<Role>> GetByNetworkIdAsync(Guid networkId)
        {
            if (await networkService.GetByIdAsync(networkId) == null) throw new NotFoundHttpException("Network not found");
            
            var roles = await repository.GetByNetworkIdAsync(networkId);
            return roles;
        }

        public async Task<Role?> GetByIdAsync(Guid networkId, Guid roleId)
        {
            if (await networkService.GetByIdAsync(networkId) == null) throw new NotFoundHttpException("Network not found");
            
            var role = await repository.GetByIdAsync(networkId, roleId);
            return role;
        }

        public async Task<Role> CreateAsync(Guid networkId, CreateRoleDto createRole)
        {
            if (await networkService.GetByIdAsync(networkId) == null) throw new NotFoundHttpException("Network not found");
            
            var role = mapper.Map<Role>(createRole);
            role.NetworkId = networkId;
            
            role = await repository.CreateAsync(role);
            return role;
        }

        public async Task<Role> UpdateAsync(Guid networkId, Guid roleId, UpdateRoleDto updateRole)
        {
            if (await networkService.GetByIdAsync(networkId) == null) throw new NotFoundHttpException("Network not found");
            
            if (!await repository.ExistsAsync(roleId)) throw new NotFoundHttpException();
            
            var role = mapper.Map<Role>(updateRole);
            role.Id = roleId;
            role.NetworkId = networkId;
            
            role = await repository.UpdateAsync(networkId, role);
            return role;
        }

        public async Task DeleteByIdAsync(Guid networkId, Guid roleId)
        {
            var role = await GetByIdAsync(networkId, roleId);
            if (role != null) await repository.DeleteAsync(role);
        }
    }
}
