using trojoaccounthub.Models;

namespace trojoaccounthub.Interfaces.Repositories
{
    public interface INetworkUserRepository : IBaseRepository
    {
        bool Exists(int id);
        ICollection<NetworkUser> GetAll();
        ICollection<NetworkUser> GetByNetworkId(int id);
        ICollection<NetworkUser> GetByUserProxyId(int id);
        NetworkUser? GetById(int id);
        NetworkUser? Create(NetworkUser networkUser);
        NetworkUser? Update(NetworkUser networkUser);
        bool Delete(NetworkUser networkUser);
    }
}
