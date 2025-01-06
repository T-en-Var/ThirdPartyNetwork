using trojoaccounthub.Models;

namespace trojoaccounthub.Interfaces.Repositories
{
    public interface INetworkUserPermissionRepository : IBaseRepository
    {
        bool Exists(int networkUserId, int permissionId);
        ICollection<NetworkUserPermission> GetAll();
        ICollection<NetworkUserPermission> GetByNetworkUserId(int id);
        ICollection<NetworkUserPermission> GetByPermissionId(int id);
        NetworkUserPermission? GetByIds(int networkUserId, int permissionId);
        NetworkUserPermission? Create(NetworkUserPermission networkUserPermission);
        NetworkUserPermission? Update(NetworkUserPermission networkUserPermission);
        bool Delete(NetworkUserPermission networkUserPermission);
    }
}
