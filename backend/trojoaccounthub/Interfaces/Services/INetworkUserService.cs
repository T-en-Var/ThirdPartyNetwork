namespace trojoaccounthub.Interfaces.Services
{
    public interface INetworkUserService : IBaseService
    {
        ICollection<NetworkUserDto> GetAll();
        ICollection<NetworkUserDto> GetByNetworkId(int id);
        ICollection<NetworkUserDto> GetByuserProxyId(int id);
        NetworkUserDto? GetById(int id);
        NetworkUserDto? Create(int networkId, NetworkUserDto networkUser);
        NetworkUserDto? Update(int id, NetworkUserDto networkUser);
        bool DeleteById(int id);
    }
}
