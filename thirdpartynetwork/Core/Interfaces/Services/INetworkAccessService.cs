﻿using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkAccessService : IBaseService
    {
        Task<ICollection<NetworkAccess>> GetAllAsync();
        Task<ICollection<NetworkAccess>> GetByNetworkIdAsync(Guid id);
        Task<ICollection<NetworkAccess>> GetByAccessIdAsync(Guid id);
        Task<NetworkAccess?> GetByIdsAsync(Guid networkId, Guid accessId);
        Task<NetworkAccess> CreateAsync(Guid networkId, Guid accessId, CreateNetworkAccessDto createNetworkAccess);
        Task<NetworkAccess> UpdateAsync(Guid networkId, Guid accessId, UpdateNetworkAccessDto updateNetworkAccess);
        Task DeleteByIdsAsync(Guid networkId, Guid accessId);
    }
}
