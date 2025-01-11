using thirdpartynetwork.Application.Dtos.NetworkUser;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkUserService : IBaseService
    {
        ICollection<NetworkUser> GetAll();
        ICollection<NetworkUser> GetByNetworkId(Guid id);
        ICollection<NetworkUser> GetByUserProxyId(Guid id);
        NetworkUser? GetById(Guid id);
        NetworkUser Create(Guid networkId, CreateNetworkUserDto createNetworkUser);
        NetworkUser Update(Guid id, UpdateNetworkUserDto updateNetworkUser);
        bool DeleteById(Guid id);
    }
}
