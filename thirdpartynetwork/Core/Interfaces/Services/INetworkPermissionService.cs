using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkPermissionService : IBaseService
    {
        ICollection<NetworkPermission> GetAll();
        ICollection<NetworkPermission> GetByNetworkId(Guid id);
        ICollection<NetworkPermission> GetByPermissionId(Guid id);
        NetworkPermission? GetByIds(Guid networkId, Guid permissionId);
        NetworkPermission Create(Guid networkId, CreateNetworkPermissionDto createNetworkPermission);
        NetworkPermission Update(Guid networkId, Guid permissionId, UpdateNetworkDto updateNetworkPermission);
        bool DeleteByIds(Guid networkId, Guid permissionId);
    }
}
