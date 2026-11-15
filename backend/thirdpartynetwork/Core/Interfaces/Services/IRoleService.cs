﻿using thirdpartynetwork.Application.Dtos.Role;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IRoleService : IBaseService
    {
        Task<ICollection<Role>> GetAllAsync();
        Task<ICollection<Role>> GetByNetworkIdAsync(Guid networkId);
        Task<ICollection<Role>> GetByUserProxyIdAsync(Guid userProxyId);
        Task<Role?> GetByIdAsync(Guid networkId, Guid roleId);
        Task<Role> CreateAsync(Guid networkId, CreateRoleDto createRole);
        Task<Role> UpdateAsync(Guid networkId, Guid roleId, UpdateRoleDto role);
        Task DeleteByIdAsync(Guid networkId, Guid roleId);
    }
}
