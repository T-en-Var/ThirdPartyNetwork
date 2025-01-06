using trojoaccounthub.Models;

namespace trojoaccounthub.Interfaces.Repositories
{
    public interface INetworkRoleRepository : IBaseRepository
    {
        bool Exists(int networkId, int roleId);
        ICollection<NetworkRole> GetAll();
        ICollection<NetworkRole> GetByNetworkId(int id);
        ICollection<NetworkRole> GetByRoleId(int id);
        NetworkRole? GetByIds(int networkId, int roleId);
        NetworkRole? Create(NetworkRole networkRole);
        NetworkRole? Update(NetworkRole networkRole);
        bool Delete(NetworkRole networkRole);
    }
}
