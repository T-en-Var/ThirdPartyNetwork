using thirdpartynetwork.Application.Dtos.NetworkUser;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkUserService : IBaseService
    {
        ICollection<NetworkUser> GetAll();
        ICollection<NetworkUser> GetByNetworkId(Guid networkId);
        ICollection<NetworkUser> GetByUserProxyId(Guid userProxyId);
        NetworkUser? GetByIds(Guid networkId, Guid userProxyId);
        NetworkUser? GetById(Guid networkId, Guid networkUserId);
        NetworkUser? GetByNetworkIdAndUsername(Guid networkId, string username);
        NetworkUser Create(Guid networkId, Guid userProxyId, CreateNetworkUserDto createNetworkUser);
        NetworkUser Update(Guid networkId, Guid networkUserId, UpdateNetworkUserDto updateNetworkUser);
        bool DeleteById(Guid networkId, Guid networkUserId);
        ICollection<Network> GetNetworksByUserProxyId(Guid userProxyId);
    }
}
