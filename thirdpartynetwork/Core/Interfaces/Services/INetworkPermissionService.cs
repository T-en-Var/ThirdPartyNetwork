using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkPermissionService : IBaseService
    {
        ICollection<NetworkPermission> GetAll();
        ICollection<NetworkPermission> GetByNetworkId(Guid id);
        ICollection<NetworkPermission> GetByPermissionId(Guid id);
        NetworkPermission? GetByIds(Guid networkId, Guid permissiondId);
        NetworkPermission Create(Guid networkId, CreateNetworkPermissionDto networkPermission);
        NetworkPermission Update(Guid networkId, Guid permissiondId, UpdateNetworkDto networkPermission);
        bool DeleteByIds(Guid networkId, Guid permissiondId);
    }
}
