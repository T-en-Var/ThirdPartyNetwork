using thirdpartynetwork.Application.Dtos.NetworkUser;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkUserService : IBaseService
    {
        ICollection<NetworkUser> GetAll();
        ICollection<NetworkUser> GetByNetworkId(Guid id);
        ICollection<NetworkUser> GetByuserProxyId(Guid id);
        NetworkUser? GetById(Guid id);
        NetworkUser Create(Guid networkId, CreateNetworkUserDto networkUser);
        NetworkUser Update(Guid id, UpdateNetworkUserDto networkUser);
        bool DeleteById(Guid id);
    }
}
