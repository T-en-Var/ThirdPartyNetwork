using thirdpartynetwork.Application.Dtos.NetworkUserPermission;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkUserPermissionService : IBaseService
    {
        ICollection<NetworkUserPermission> GetAll();
        ICollection<NetworkUserPermission> GetByNetworkId(Guid id);
        ICollection<NetworkUserPermission> GetByPermissionId(Guid id);
        NetworkUserPermission? GetByIds(Guid networkUserId, Guid permissionId);
        NetworkUserPermission Create(Guid networkId, CreateNetworkUserPermissionDto createNetworkUserPermission);
        NetworkUserPermission Update(Guid networkUserId, Guid permissionId, UpdateNetworkUserPermissionDto updateNetworkUserPermission);
        bool DeleteById(Guid id);
    }
}
