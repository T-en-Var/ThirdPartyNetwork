using thirdpartynetwork.Dtos.NetworkUserRole;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkUserRoleService : IBaseService
    {
        ICollection<NetworkUserRoleDto> GetAll();
        ICollection<NetworkUserRoleDto> GetByNetworkUserId(Guid id);
        ICollection<NetworkUserRoleDto> GetByRoleId(Guid id);
        NetworkUserRoleDto? GetByIds(Guid networkUserId, Guid roleId);
        NetworkUserRoleDto? Create(Guid networkId, CreateNetworkUserRoleDto networkUserRole);
        NetworkUserRoleDto? Update(Guid networkId, Guid roleId, UpdateNetworkUserRoleDto networkUserRole);
        bool DeleteById(Guid id);
    }
}
