﻿using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkUserRoleService(
        INetworkUserRoleRepository repository, 
        INetworkService networkService, 
        INetworkUserService networkUserService, 
        IRoleService roleService, 
        IMapper mapper) : INetworkUserRoleService
    {
        public async Task<ICollection<NetworkUserRole>> GetAllAsync()
        {
            var networkUserRoles = await repository.GetAllAsync();
            return networkUserRoles;
        }

        public async Task<ICollection<NetworkUserRole>> GetByNetworkUserIdAsync(Guid networkId, Guid networkUserId)
        {
            if (await networkService.GetByIdAsync(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (await networkUserService.GetByIdAsync(networkId, networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            
            var networkUserRoles = await repository.GetByNetworkUserIdAsync(networkId, networkUserId);
            return networkUserRoles;
        }

        public async Task<ICollection<NetworkUserRole>> GetByRoleIdAsync(Guid networkId, Guid roleId)
        {
            if (await roleService.GetByIdAsync(networkId, roleId) == null) throw new NotFoundHttpException("Role not found");
            
            var networkUserRoles = await repository.GetByRoleIdAsync(networkId, roleId);
            return networkUserRoles;
        }

        public async Task<NetworkUserRole?> GetByIdsAsync(Guid networkId, Guid networkUserId, Guid roleId)
        {
            if (await networkService.GetByIdAsync(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (await networkUserService.GetByIdAsync(networkId, networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            if (await roleService.GetByIdAsync(networkId, roleId) == null) throw new NotFoundHttpException("Role not found");

            var networkUserRole = await repository.GetByIdsAsync(networkId, networkUserId, roleId);
            return networkUserRole;
        }

        public async Task<NetworkUserRole> CreateAsync(Guid networkId, Guid networkUserId, Guid roleId, CreateNetworkUserRoleDto createNetworkUserRole)
        {
            if (await networkService.GetByIdAsync(networkId) == null) throw new NotFoundHttpException("Network not found");
            if (await networkUserService.GetByIdAsync(networkId, networkUserId) == null) throw new NotFoundHttpException("NetworkUser not found");
            if (await roleService.GetByIdAsync(networkId, roleId) == null) throw new NotFoundHttpException("Role not found");
                
            if (await repository.ExistsAsync(networkId, networkUserId, roleId)) throw new ConflictHttpException();
            
            var networkUserRole = mapper.Map<NetworkUserRole>(createNetworkUserRole);
            networkUserRole.NetworkUserId = networkUserId;
            networkUserRole.RoleId = roleId;
            
            networkUserRole = await repository.CreateAsync(networkUserRole);
            return networkUserRole;
        }

        public async Task<NetworkUserRole> UpdateAsync(Guid networkId, Guid networkUserId, Guid roleId, UpdateNetworkUserRoleDto updateNetworkUserRole)
        {
            if (!await repository.ExistsAsync(networkId, networkUserId, roleId)) throw new NotFoundHttpException();
            
            var networkUserRole = mapper.Map<NetworkUserRole>(updateNetworkUserRole);
            networkUserRole.NetworkUserId = networkUserId;
            networkUserRole.RoleId = roleId;
            
            networkUserRole = await repository.UpdateAsync(networkId, networkUserRole);
            return networkUserRole;
        }

        public async Task DeleteByIdsAsync(Guid networkId, Guid networkUserId, Guid roleId)
        {
            var networkUserRole = await repository.GetByIdsAsync(networkId, networkUserId, roleId);
            if (networkUserRole != null) await repository.DeleteAsync(networkUserRole);
        }
    }
}
