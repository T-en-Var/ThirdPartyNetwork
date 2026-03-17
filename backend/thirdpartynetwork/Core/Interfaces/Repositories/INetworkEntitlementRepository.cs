using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface INetworkEntitlementRepository : IBaseRepository
    {
        Task<bool> ExistsAsync(Guid id);
        Task<NetworkEntitlement?> GetByIdAsync(Guid id);
        Task<NetworkEntitlement> UpdateAsync(NetworkEntitlement networkEntitlement);
    }
}
