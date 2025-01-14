using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface INetworkUserRoleRepository : IBaseRepository
    {
        bool Exists(Guid networkUserId, Guid roleId);
        ICollection<NetworkUserRole> GetAll();
        ICollection<NetworkUserRole> GetByNetworkUserId(Guid id);
        ICollection<NetworkUserRole> GetByRoleId(Guid id);
        NetworkUserRole? GetByIds(Guid networkUserId, Guid roleId);
        NetworkUserRole? GetByIdAndName(Guid networkUserId, string roleName);
        NetworkUserRole Create(NetworkUserRole networkUserRole);
        NetworkUserRole Update(NetworkUserRole networkUserRole);
        bool Delete(NetworkUserRole networkUserRole);
    }
}
