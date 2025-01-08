using thirdpartynetwork.Models;

namespace thirdpartynetwork.Interfaces.Repositories
{
    public interface INetworkUserPermissionRepository : IBaseRepository
    {
        bool Exists(Guid networkUserId, Guid permissionId);
        ICollection<NetworkUserPermission> GetAll();
        ICollection<NetworkUserPermission> GetByNetworkUserId(Guid id);
        ICollection<NetworkUserPermission> GetByPermissionId(Guid id);
        NetworkUserPermission? GetByIds(Guid networkUserId, Guid permissionId);
        NetworkUserPermission? Create(NetworkUserPermission networkUserPermission);
        NetworkUserPermission? Update(NetworkUserPermission networkUserPermission);
        bool Delete(NetworkUserPermission networkUserPermission);
    }
}
