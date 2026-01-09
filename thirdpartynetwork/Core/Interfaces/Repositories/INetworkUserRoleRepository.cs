﻿using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface INetworkUserRoleRepository : IBaseRepository
    {
        Task<bool> ExistsAsync(Guid networkId, Guid networkUserId, Guid roleId);
        Task<ICollection<NetworkUserRole>> GetAllAsync();
        Task<ICollection<NetworkUserRole>> GetByNetworkUserIdAsync(Guid networkId, Guid id);
        Task<ICollection<NetworkUserRole>> GetByRoleIdAsync(Guid networkId, Guid id);
        Task<NetworkUserRole?> GetByIdsAsync(Guid networkId, Guid networkUserId, Guid roleId);
        Task<NetworkUserRole?> GetByIdAndNameAsync(Guid networkId, Guid networkUserId, string roleName);
        Task<NetworkUserRole> CreateAsync(NetworkUserRole networkUserRole);
        Task<NetworkUserRole> UpdateAsync(Guid networkId, NetworkUserRole networkUserRole);
        Task DeleteAsync(NetworkUserRole networkUserRole);
    }
}
