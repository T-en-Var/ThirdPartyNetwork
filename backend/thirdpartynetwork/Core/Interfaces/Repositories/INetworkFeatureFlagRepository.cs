using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface INetworkFeatureFlagRepository : IBaseRepository
    {
        Task<bool> ExistsAsync(Guid id);
        Task<NetworkFeatureFlag?> GetByIdAsync(Guid id);
        Task<NetworkFeatureFlag> UpdateAsync(NetworkFeatureFlag networkFeatureFlag);
    }
}
