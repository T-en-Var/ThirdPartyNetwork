﻿using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface INetworkAccessRepository : IBaseRepository
    {
        Task<bool> ExistsAsync(Guid networkId, Guid accessId);
        Task<ICollection<NetworkAccess>> GetAllAsync();
        Task<ICollection<NetworkAccess>> GetByNetworkIdAsync(Guid id);
        Task<ICollection<NetworkAccess>> GetByAccessIdAsync(Guid id);
        Task<NetworkAccess?> GetByIdsAsync(Guid networkId, Guid accessId);
        Task<NetworkAccess> CreateAsync(NetworkAccess networkAccess);
        Task<NetworkAccess> UpdateAsync(NetworkAccess networkAccess);
        Task DeleteAsync(NetworkAccess networkAccess);
    }
}
