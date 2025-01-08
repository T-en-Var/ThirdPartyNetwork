using thirdpartynetwork.Dtos.NetworkUserPermission;

namespace thirdpartynetwork.Interfaces.Services
{
    public interface INetworkUserPermissionService : IBaseService
    {
        ICollection<NetworkUserPermissionDto> GetAll();
        ICollection<NetworkUserPermissionDto> GetByNetworkId(Guid id);
        ICollection<NetworkUserPermissionDto> GetByPermissionId(Guid id);
        NetworkUserPermissionDto? GetByIds(Guid networkUserId, Guid permissionId);
        NetworkUserPermissionDto? Create(Guid networkId, CreateNetworkUserPermissionDto networkUserPermission);
        NetworkUserPermissionDto? Update(Guid networkUserId, Guid permissionId, UpdateNetworkUserPermissionDto networkUserPermission);
        bool DeleteById(Guid id);
    }
}
