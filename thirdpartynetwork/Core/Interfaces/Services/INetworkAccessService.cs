using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkAccessService : IBaseService
    {
        ICollection<NetworkAccess> GetAll();
        ICollection<NetworkAccess> GetByNetworkId(Guid id);
        ICollection<NetworkAccess> GetByAccessId(Guid id);
        NetworkAccess? GetByIds(Guid networkId, Guid accessId);
        NetworkAccess Create(Guid networkId, Guid accessId, CreateNetworkAccessDto createNetworkAccess);
        NetworkAccess Update(Guid networkId, Guid accessId, UpdateNetworkAccessDto updateNetworkAccess);
        bool DeleteByIds(Guid networkId, Guid accessId);
    }
}
