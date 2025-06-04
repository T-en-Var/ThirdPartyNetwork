using thirdpartynetwork.Application.Dtos.NetworkUserAccess;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkUserAccessService : IBaseService
    {
        ICollection<NetworkUserAccess> GetAll();
        ICollection<NetworkUserAccess> GetByNetworkId(Guid networkId, Guid networkUserId);
        ICollection<NetworkUserAccess> GetByAccessId(Guid accessId);
        void Populate(Guid networkId, Guid accessId);
        void Depopulate(Guid networkId, Guid accessId);
        NetworkUserAccess? GetByIds(Guid networkId, Guid networkUserId, Guid accessId);
        NetworkUserAccess Create(Guid networkId, Guid networkUserId, Guid accessId, CreateNetworkUserAccessDto createNetworkUserAccess);
        NetworkUserAccess Update(Guid networkId, Guid networkUserId, Guid accessId, UpdateNetworkUserAccessDto updateNetworkUserAccess);
        bool DeleteByIds(Guid networkId, Guid networkUserId, Guid accessId);
    }
}
