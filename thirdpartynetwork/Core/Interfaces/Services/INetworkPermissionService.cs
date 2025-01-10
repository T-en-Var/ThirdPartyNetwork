using thirdpartynetwork.Application.Dtos.Network;
using thirdpartynetwork.Application.Dtos.NetworkPermission;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkPermissionService : IBaseService
    {
        ICollection<NetworkPermissionDto> GetAll();
        ICollection<NetworkPermissionDto> GetByNetworkId(Guid id);
        ICollection<NetworkPermissionDto> GetByPermissionId(Guid id);
        NetworkPermissionDto? GetByIds(Guid networkId, Guid permissiondId);
        NetworkPermissionDto? Create(Guid networkId, CreateNetworkPermissionDto networkPermission);
        NetworkPermissionDto? Update(Guid networkId, Guid permissiondId, UpdateNetworkDto networkPermission);
        bool DeleteByIds(Guid networkId, Guid permissiondId);
    }
}
