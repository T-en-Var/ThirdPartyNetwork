using trojoaccounthub.Models;

namespace trojoaccounthub.Interfaces.Repositories
{
    public interface INetworkUserRoleRepository : IBaseRepository
    {
        bool Exists(int networkUserId, int roleId);
        ICollection<NetworkUserRole> GetAll();
        ICollection<NetworkUserRole> GetByNetworkUserId(int id);
        ICollection<NetworkUserRole> GetByRoleId(int id);
        NetworkUserRole? GetByIds(int networkUserId, int roleId);
        NetworkUserRole? Create(NetworkUserRole networkUserRole);
        NetworkUserRole? Update(NetworkUserRole networkUserRole);
        bool Delete(NetworkUserRole networkUserRole);
    }
}
