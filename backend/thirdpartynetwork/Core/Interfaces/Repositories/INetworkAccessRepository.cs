using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface INetworkAccessRepository : IBaseRepository
    {
        bool Exists(Guid networkId, Guid accessId);
        ICollection<NetworkAccess> GetAll();
        ICollection<NetworkAccess> GetByNetworkId(Guid id);
        ICollection<NetworkAccess> GetByAccessId(Guid id);
        NetworkAccess? GetByIds(Guid networkId, Guid accessId);
        NetworkAccess Create(NetworkAccess networkAccess);
        NetworkAccess Update(NetworkAccess networkAccess);
        void Delete(NetworkAccess networkAccess);
    }
}
