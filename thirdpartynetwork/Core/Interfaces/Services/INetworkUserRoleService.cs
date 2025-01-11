using thirdpartynetwork.Application.Dtos.NetworkUserRole;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkUserRoleService : IBaseService
    {
        ICollection<NetworkUserRole> GetAll();
        ICollection<NetworkUserRole> GetByNetworkUserId(Guid id);
        ICollection<NetworkUserRole> GetByRoleId(Guid id);
        NetworkUserRole? GetByIds(Guid networkUserId, Guid roleId);
        NetworkUserRole Create(Guid networkId, CreateNetworkUserRoleDto networkUserRole);
        NetworkUserRole Update(Guid networkId, Guid roleId, UpdateNetworkUserRoleDto networkUserRole);
        bool DeleteById(Guid id);
    }
}
