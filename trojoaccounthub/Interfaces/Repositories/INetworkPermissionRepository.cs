using trojoaccounthub.Models;

namespace trojoaccounthub.Interfaces.Repositories
{
    public interface INetworkPermissionRepository : IBaseRepository
    {
        bool Exists(int networkId, int permissionId);
        ICollection<NetworkPermission> GetAll();
        ICollection<NetworkPermission> GetByNetworkId(int id);
        ICollection<NetworkPermission> GetByPermissionId(int id);
        NetworkPermission? GetByIds(int networkId, int permissiondId);
        NetworkPermission? Create(NetworkPermission networkPermission);
        NetworkPermission? Update(NetworkPermission networkPermission);
        bool Delete(NetworkPermission networkPermission);
    }
}
