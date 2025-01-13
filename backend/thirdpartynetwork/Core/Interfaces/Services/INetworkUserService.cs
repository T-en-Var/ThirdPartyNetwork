using thirdpartynetwork.Application.Dtos.NetworkUser;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkUserService : IBaseService
    {
        ICollection<NetworkUser> GetAll();
        ICollection<NetworkUser> GetByNetworkId(Guid networkId);
        ICollection<NetworkUser> GetByUserProxyId(Guid userProxyId);
        NetworkUser? GetById(Guid networkUserId);
        NetworkUser Create(Guid networkId, Guid userProxyId, CreateNetworkUserDto createNetworkUser);
        NetworkUser Update(Guid networkId, Guid networkUserId, UpdateNetworkUserDto updateNetworkUser);
        bool DeleteById(Guid networkUserId);
    }
}
