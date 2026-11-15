﻿using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface INetworkRepository : IBaseRepository
    {
        Task<bool> ExistsAsync(Guid id);
        Task<ICollection<Network>> GetAllAsync();
        Task<ICollection<NetworkMetricsDto>> GetWithMetricsAsync(Guid? networkId = null);
        Task<Network?> GetByIdAsync(Guid id, bool bypass = false);
        Task<Network?> GetDetailsByIdAsync(Guid id);
        Task<Network> GetMainNetworkAsync();
        Task<Network> GetMainNetworkDetailsAsync();
        Task<Network?> GetByNameAsync(string name);
        IQueryable<Network> GetByNameQueryable(string name);
        IQueryable<Network> GetByIdQueryable(Guid id);
        Task<Network?> GetByIdFeatureFlagAsync(Guid id);
        Task<Network> CreateAsync(Network network);
        Task<Network> UpdateAsync(Network network);
        Task DeleteAsync(Network network);
    }
}
