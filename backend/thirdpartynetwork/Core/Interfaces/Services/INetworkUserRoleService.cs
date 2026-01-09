﻿using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkUserRoleService : IBaseService
    {
        Task<ICollection<NetworkUserRole>> GetAllAsync();
        Task<ICollection<NetworkUserRole>> GetByNetworkUserIdAsync(Guid networkId, Guid networkUserId);
        Task<ICollection<NetworkUserRole>> GetByRoleIdAsync(Guid networkId, Guid roleId);
        Task<NetworkUserRole?> GetByIdsAsync(Guid networkId, Guid networkUserId, Guid roleId);
        Task<NetworkUserRole> CreateAsync(Guid networkId, Guid networkUserId, Guid roleId, CreateNetworkUserRoleDto createNetworkUserRole);
        Task<NetworkUserRole> UpdateAsync(Guid networkId, Guid networkUserId, Guid roleId, UpdateNetworkUserRoleDto updateNetworkUserRole);
        Task DeleteByIdsAsync(Guid networkId, Guid networkUserId, Guid roleId);
    }
}
