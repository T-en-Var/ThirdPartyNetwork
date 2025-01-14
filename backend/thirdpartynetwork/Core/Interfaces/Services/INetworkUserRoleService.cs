using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkUserRoleService : IBaseService
    {
        ICollection<NetworkUserRole> GetAll();
        ICollection<NetworkUserRole> GetByNetworkUserId(Guid networkId, Guid networkUserId);
        ICollection<NetworkUserRole> GetByRoleId(Guid networkId, Guid roleId);
        NetworkUserRole? GetByIds(Guid networkId, Guid networkUserId, Guid roleId);
        NetworkUserRole Create(Guid networkId, Guid networkUserId, Guid roleId, CreateNetworkUserRoleDto createNetworkUserRole);
        NetworkUserRole Update(Guid networkId, Guid networkUserId, Guid roleId, UpdateNetworkUserRoleDto updateNetworkUserRole);
        bool DeleteByIds(Guid networkId, Guid networkUserId, Guid roleId);
    }
}
