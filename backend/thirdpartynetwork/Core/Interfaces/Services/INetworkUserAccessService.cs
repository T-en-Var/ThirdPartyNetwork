﻿using thirdpartynetwork.Application.Dtos.NetworkUserAccess;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkUserAccessService : IBaseService
    {
        Task<ICollection<NetworkUserAccess>> GetAllAsync();
        Task<ICollection<NetworkUserAccess>> GetByNetworkIdAsync(Guid networkId, Guid networkUserId);
        Task<ICollection<NetworkUserAccess>> GetByAccessIdAsync(Guid accessId);
        Task PopulateAsync(Guid networkId, Guid accessId);
        Task DepopulateAsync(Guid networkId, Guid accessId);
        Task<NetworkUserAccess?> GetByIdsAsync(Guid networkId, Guid networkUserId, Guid accessId);
        Task<NetworkUserAccess> CreateAsync(Guid networkId, Guid networkUserId, Guid accessId, CreateNetworkUserAccessDto createNetworkUserAccess);
        Task<NetworkUserAccess> UpdateAsync(Guid networkId, Guid networkUserId, Guid accessId, UpdateNetworkUserAccessDto updateNetworkUserAccess);
        Task DeleteByIdsAsync(Guid networkId, Guid networkUserId, Guid accessId);
    }
}
