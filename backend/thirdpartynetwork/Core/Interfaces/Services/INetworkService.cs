using thirdpartynetwork.Application.Dtos;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkService : IBaseService
    {
        Task<ICollection<Network>> GetAllAsync();
        Task<ICollection<NetworkMetricsDto>> GetAllWithMetricsAsync();
        Task<NetworkMetricsDto> GetByIdWithMetricsAsync(Guid networkId);
        Task<Network?> GetByIdAsync(Guid networkId, bool bypass = false);
        Task<Network?> GetDetailsByIdAsync(Guid networkId);
        Task<Network> CreateAsync(CreateNetworkDto createNetworkDto);
        Task<Network> UpdateAsync(Guid id, UpdateNetworkDto updateNetworkDto);
        Task<NetworkFeatureFlag> UpdateFeatureFlagAsync(Guid id, UpdateNetworkFeatureFlagDto updateNetworkDto);
        Task DeleteByIdAsync(Guid id);
        Task<Network> GetMainNetworkAsync();
        Task<Network> GetMainNetworkDetailsAsync();
        IQueryable<Network> GetMainNetworkQueryable();
        IQueryable<Network> GetByIdQueryable(Guid networkId);
    }
}
