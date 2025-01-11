using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface INetworkUserRepository : IBaseRepository
    {
        bool Exists(Guid id);
        ICollection<NetworkUser> GetAll();
        ICollection<NetworkUser> GetByNetworkId(Guid id);
        ICollection<NetworkUser> GetByUserProxyId(Guid id);
        NetworkUser? GetById(Guid id);
        NetworkUser Create(NetworkUser networkUser);
        NetworkUser Update(NetworkUser networkUser);
        bool Delete(NetworkUser networkUser);
    }
}
