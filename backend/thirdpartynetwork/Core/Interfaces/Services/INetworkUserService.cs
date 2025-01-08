using thirdpartynetwork.Dtos.NetworkUser;

namespace thirdpartynetwork.Interfaces.Services
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
