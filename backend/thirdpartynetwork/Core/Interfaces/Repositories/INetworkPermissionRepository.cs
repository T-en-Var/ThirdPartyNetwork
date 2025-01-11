using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface INetworkPermissionRepository : IBaseRepository
    {
        bool Exists(Guid networkId, Guid permissionId);
        ICollection<NetworkPermission> GetAll();
        ICollection<NetworkPermission> GetByNetworkId(Guid id);
        ICollection<NetworkPermission> GetByPermissionId(Guid id);
        NetworkPermission? GetByIds(Guid networkId, Guid permissiondId);
        NetworkPermission Create(NetworkPermission networkPermission);
        NetworkPermission Update(NetworkPermission networkPermission);
        bool Delete(NetworkPermission networkPermission);
    }
}
