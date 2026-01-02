using thirdpartynetwork.Application.Dtos.NetworkUser;
using thirdpartynetwork.Core.Models;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkUserService : IBaseService
    {
        ICollection<NetworkUser> GetAll();
        ICollection<NetworkUser> GetByNetworkId(Guid networkId);
        ICollection<NetworkUser> GetByUserProxyId(Guid userProxyId);
        NetworkUser? GetByIds(Guid networkId, Guid userProxyId);
        NetworkUser? GetByIdsPreAuth(Guid networkId, Guid userProxyId);
        NetworkUser? GetById(Guid networkId, Guid networkUserId);
        NetworkUser? GetJustById(Guid networkUserId);
        NetworkUser? GetByNetworkIdAndEmail(Guid networkId, string email);
        NetworkUser Create(Guid networkId, Guid userProxyId, CreateNetworkUserDto createNetworkUser);
        NetworkUser Update(Guid networkId, Guid networkUserId, UpdateNetworkUserDto updateNetworkUser);
        void NetworkAccessUpdate(Guid networkId);
        void DeleteById(Guid networkId, Guid networkUserId);
        ICollection<Network> GetNetworksByUserProxyId(Guid userProxyId);
        ICollection<File> GetFilesByUserProxyId(Guid userProxyId);
    }
}
