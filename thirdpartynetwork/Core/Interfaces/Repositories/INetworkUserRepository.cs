using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface INetworkUserRepository : IBaseRepository
    {
        bool Exists(Guid id);
        ICollection<NetworkUser> GetAll();
        ICollection<NetworkUser> GetByNetworkId(Guid networkId);
        ICollection<NetworkUser> GetByUserProxyId(Guid userProxyId);
        NetworkUser? GetById(Guid networkUserId);
        NetworkUser? GetByNetworkIdAndUsername(Guid networkId, string username);
        NetworkUser? GetByIds(Guid networkId, Guid userProxyId);
        NetworkUser Create(NetworkUser networkUser);
        NetworkUser Update(NetworkUser networkUser);
        bool Delete(NetworkUser networkUser);
    }
}
