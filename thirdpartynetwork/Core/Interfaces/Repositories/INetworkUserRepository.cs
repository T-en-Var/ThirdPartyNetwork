using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface INetworkUserRepository : IBaseRepository
    {
        bool Exists(Guid id);
        ICollection<NetworkUser> GetAll();
        ICollection<NetworkUser> GetByNetworkId(Guid networkId);
        ICollection<NetworkUser> GetByUserProxyId(Guid userProxyId, Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>? queryConfig = null);
        NetworkUser? GetById(Guid networkId, Guid networkUserId);
        NetworkUser? GetJustById(Guid networkUserId);
        NetworkUser? GetByNetworkIdAndEmail(Guid networkId, string email);
        NetworkUser? GetByIds(Guid networkId, Guid userProxyId);
        NetworkUser? GetByIdsPreAuth(Guid networkId, Guid userProxyId);
        NetworkUser Create(NetworkUser networkUser);
        NetworkUser Update(Guid networkId, NetworkUser networkUser);
        void NetworkAccessUpdate(Guid networkId);
        void Delete(NetworkUser networkUser);
    }
}
