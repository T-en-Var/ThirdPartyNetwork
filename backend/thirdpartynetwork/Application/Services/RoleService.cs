using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class RoleService(IRoleRepository repository, INetworkService networkService, IUserProxyService userProxyService, IMapper mapper) : IRoleService
    {
        private static ICollection<Role> MultiApplyDefaults(ICollection<Role> roles)
        {
            foreach (var role in roles) SingleApplyDefaults(role);

            return roles;
        }

        private static Role? SingleApplyDefaults(Role? role)
        {
            if (role == null) return null;
            foreach (var userRole in role.NetworkUserRoles)
                userRole.NetworkUser.UserProxy.ApplyDefaults();

            return role;
        }

        public async Task<ICollection<Role>> GetAllAsync()
        {
            var roles = await repository.GetAllAsync();
            return roles;
        }

        public async Task<ICollection<Role>> GetByNetworkIdAsync(Guid networkId)
        {
            if (await networkService.GetByIdAsync(networkId) == null)
                throw new NotFoundHttpException("Network not found");

            var roles = await repository.GetByNetworkIdAsync(networkId);
            
            return MultiApplyDefaults(roles);
        }

        public async Task<ICollection<Role>> GetByUserProxyIdAsync(Guid userProxyId)
        {
            if (await userProxyService.GetByIdAsync(userProxyId) == null)
                throw new NotFoundHttpException("UserProxy not found");

            var roles = await repository.GetByUserProxyIdAsync(userProxyId);
            
            return MultiApplyDefaults(roles);
        }

        public async Task<Role?> GetByIdAsync(Guid networkId, Guid roleId)
        {
            if (await networkService.GetByIdAsync(networkId) == null)
                throw new NotFoundHttpException("Network not found");

            var role = await repository.GetByIdAsync(networkId, roleId);
            return SingleApplyDefaults(role);
        }

        public async Task<Role> CreateAsync(Guid networkId, CreateRoleDto createRole)
        {
            if (await networkService.GetByIdAsync(networkId) == null)
                throw new NotFoundHttpException("Network not found");

            var role = mapper.Map<Role>(createRole);
            role.NetworkId = networkId;

            role = await repository.CreateAsync(role);
            return SingleApplyDefaults(role)!;
        }

        public async Task<Role> UpdateAsync(Guid networkId, Guid roleId, UpdateRoleDto updateRole)
        {
            if (await networkService.GetByIdAsync(networkId) == null)
                throw new NotFoundHttpException("Network not found");

            if (!await repository.ExistsAsync(roleId)) throw new NotFoundHttpException();

            var role = mapper.Map<Role>(updateRole);
            role.Id = roleId;
            role.NetworkId = networkId;

            role = await repository.UpdateAsync(networkId, role);
            return SingleApplyDefaults(role)!;
        }

        public async Task DeleteByIdAsync(Guid networkId, Guid roleId)
        {
            var role = await GetByIdAsync(networkId, roleId);
            if (role != null) await repository.DeleteAsync(role);
        }
    }
}