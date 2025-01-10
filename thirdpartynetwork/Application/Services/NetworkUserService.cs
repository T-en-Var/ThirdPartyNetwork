namespace thirdpartynetwork.Application.Services
{
    public class NetworkUserService : INetworkUserService
    {
        public ICollection<NetworkUserDto> GetAll()
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkUserDto> GetByNetworkId(Guid id)
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkUserDto> GetByuserProxyId(Guid id)
        {
            throw new NotImplementedException();
        }

        public NetworkUserDto? GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public NetworkUserDto? Create(Guid networkId, NetworkUserDto networkUser)
        {
            throw new NotImplementedException();
        }

        public NetworkUserDto? Update(Guid id, NetworkUserDto networkUser)
        {
            throw new NotImplementedException();
        }

        public bool DeleteById(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
