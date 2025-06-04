using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface INetworkUserAccessRepository : IBaseRepository
    {
        bool Exists(Guid networkUserId, Guid accessId);
        ICollection<NetworkUserAccess> GetAll();
        ICollection<NetworkUserAccess> GetByNetworkUserId(Guid id);
        ICollection<NetworkUserAccess> GetByAccessId(Guid id);
        void Populate(Guid networkId, Guid accessId);
        void Depopulate(Guid networkId, Guid accessId);
        NetworkUserAccess? GetByIds(Guid networkUserId, Guid permissionId);
        NetworkUserAccess Create(NetworkUserAccess networkUserPermission);
        NetworkUserAccess Update(NetworkUserAccess networkUserPermission);
        bool Delete(NetworkUserAccess networkUserPermission);
    }
}
