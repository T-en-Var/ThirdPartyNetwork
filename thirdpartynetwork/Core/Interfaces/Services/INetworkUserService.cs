﻿using thirdpartynetwork.Application.Dtos.NetworkUser;
using thirdpartynetwork.Core.Models;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkUserService : IBaseService
    {
        Task<ICollection<NetworkUser>> GetAllAsync();
        Task<ICollection<NetworkUser>> GetByNetworkIdAsync(Guid networkId);
        Task<ICollection<NetworkUser>> GetByUserProxyIdAsync(Guid userProxyId);
        Task<NetworkUser?> GetByIdsAsync(Guid networkId, Guid userProxyId);
        Task<NetworkUser?> GetByIdsPreAuthAsync(Guid networkId, Guid userProxyId);
        Task<NetworkUser?> GetByIdAsync(Guid networkId, Guid networkUserId);
        Task<NetworkUser?> GetJustByIdAsync(Guid networkUserId);
        Task<NetworkUser?> GetByNetworkIdAndEmailAsync(Guid networkId, string email);
        Task<NetworkUser> CreateAsync(Guid networkId, Guid userProxyId, CreateNetworkUserDto createNetworkUser);
        Task<NetworkUser> UpdateAsync(Guid networkId, Guid networkUserId, UpdateNetworkUserDto updateNetworkUser);
        Task NetworkAccessUpdateAsync(Guid networkId);
        Task DeleteByIdAsync(Guid networkId, Guid networkUserId);
        Task<ICollection<Network>> GetNetworksByUserProxyIdAsync(Guid userProxyId);
        Task<ICollection<File>> GetFilesByUserProxyIdAsync(Guid userProxyId);
    }
}
