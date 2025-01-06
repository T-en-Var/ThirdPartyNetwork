using thirdpartynetwork.Models;

namespace thirdpartynetwork.Interfaces.Repositories
{
    public interface INetworkRoleRepository : IBaseRepository
    {
        bool Exists(Guid networkId, Guid roleId);
        ICollection<NetworkRole> GetAll();
        ICollection<NetworkRole> GetByNetworkId(Guid id);
        ICollection<NetworkRole> GetByRoleId(Guid id);
        NetworkRole? GetByIds(Guid networkId, Guid roleId);
        NetworkRole? Create(NetworkRole networkRole);
        NetworkRole? Update(NetworkRole networkRole);
        bool Delete(NetworkRole networkRole);
    }
}
