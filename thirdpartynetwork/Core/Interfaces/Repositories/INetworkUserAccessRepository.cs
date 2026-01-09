﻿using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface INetworkUserAccessRepository : IBaseRepository
    {
        Task<bool> ExistsAsync(Guid networkUserId, Guid accessId);
        Task<ICollection<NetworkUserAccess>> GetAllAsync();
        Task<ICollection<NetworkUserAccess>> GetByNetworkUserIdAsync(Guid id);
        Task<ICollection<NetworkUserAccess>> GetByAccessIdAsync(Guid id);
        Task PopulateAsync(Guid networkId, Guid accessId);
        Task DepopulateAsync(Guid networkId, Guid accessId);
        Task<NetworkUserAccess?> GetByIdsAsync(Guid networkUserId, Guid permissionId);
        Task<NetworkUserAccess> CreateAsync(NetworkUserAccess networkUserPermission);
        Task<NetworkUserAccess> UpdateAsync(NetworkUserAccess networkUserPermission);
        Task MarkAccessCompleteAsync(Guid networkUserId);
        Task DeleteAsync(NetworkUserAccess networkUserPermission);
    }
}
