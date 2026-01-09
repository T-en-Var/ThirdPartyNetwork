using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface INetworkUserRepository : IBaseRepository
    {
        Task<bool> ExistsAsync(Guid id);
        Task<ICollection<NetworkUser>> GetAllAsync();
        Task<ICollection<NetworkUser>> GetByNetworkIdAsync(Guid networkId);
        Task<ICollection<NetworkUser>> GetByUserProxyIdAsync(Guid userProxyId, Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>? queryConfig = null);
        Task<NetworkUser?> GetByIdAsync(Guid networkId, Guid networkUserId);
        Task<NetworkUser?> GetJustByIdAsync(Guid networkUserId);
        Task<NetworkUser?> GetByNetworkIdAndEmailAsync(Guid networkId, string email);
        Task<NetworkUser?> GetByIdsAsync(Guid networkId, Guid userProxyId);
        Task<NetworkUser?> GetByIdsPreAuthAsync(Guid networkId, Guid userProxyId);
        Task<NetworkUser> CreateAsync(NetworkUser networkUser);
        Task<NetworkUser> UpdateAsync(Guid networkId, NetworkUser networkUser);
        Task NetworkAccessUpdateAsync(Guid networkId);
        Task DeleteAsync(NetworkUser networkUser);
    }
}
