using thirdpartynetwork.Application.Dtos.NetworkUserPermission;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkUserPermissionService : IBaseService
    {
        ICollection<NetworkUserPermission> GetAll();
        ICollection<NetworkUserPermission> GetByNetworkId(Guid networkId, Guid networkUserId);
        ICollection<NetworkUserPermission> GetByPermissionId(Guid permissionId);
        NetworkUserPermission? GetByIds(Guid networkId, Guid networkUserId, Guid permissionId);
        NetworkUserPermission Create(Guid networkId, Guid networkUserId, Guid permissionId, CreateNetworkUserPermissionDto createNetworkUserPermission);
        NetworkUserPermission Update(Guid networkId, Guid networkUserId, Guid permissionId, UpdateNetworkUserPermissionDto updateNetworkUserPermission);
        bool DeleteByIds(Guid networkId, Guid networkUserId, Guid permissionId);
    }
}
