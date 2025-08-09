using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface INetworkUserRoleRepository : IBaseRepository
    {
        bool Exists(Guid networkId, Guid networkUserId, Guid roleId);
        ICollection<NetworkUserRole> GetAll();
        ICollection<NetworkUserRole> GetByNetworkUserId(Guid networkId, Guid id);
        ICollection<NetworkUserRole> GetByRoleId(Guid networkId, Guid id);
        NetworkUserRole? GetByIds(Guid networkId, Guid networkUserId, Guid roleId);
        NetworkUserRole? GetByIdAndName(Guid networkId, Guid networkUserId, string roleName);
        NetworkUserRole Create(NetworkUserRole networkUserRole);
        NetworkUserRole Update(Guid networkId, NetworkUserRole networkUserRole);
        void Delete(NetworkUserRole networkUserRole);
    }
}
