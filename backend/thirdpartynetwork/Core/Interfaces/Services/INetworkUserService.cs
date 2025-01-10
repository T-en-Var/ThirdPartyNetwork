using thirdpartynetwork.Application.Dtos.NetworkUser;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkUserService : IBaseService
    {
        ICollection<NetworkUserDto> GetAll();
        ICollection<NetworkUserDto> GetByNetworkId(Guid id);
        ICollection<NetworkUserDto> GetByuserProxyId(Guid id);
        NetworkUserDto? GetById(Guid id);
        NetworkUserDto? Create(Guid networkId, NetworkUserDto networkUser);
        NetworkUserDto? Update(Guid id, NetworkUserDto networkUser);
        bool DeleteById(Guid id);
    }
}
