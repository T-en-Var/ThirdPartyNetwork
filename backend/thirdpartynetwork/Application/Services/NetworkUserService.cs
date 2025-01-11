using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkUserService : INetworkUserService
    {
        public ICollection<NetworkUser> GetAll()
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkUser> GetByNetworkId(Guid id)
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkUser> GetByUserProxyId(Guid id)
        {
            throw new NotImplementedException();
        }

        public NetworkUser? GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public NetworkUser Create(Guid networkId, CreateNetworkUserDto createNetworkUser)
        {
            throw new NotImplementedException();
        }

        public NetworkUser Update(Guid id, UpdateNetworkUserDto updateNetworkUser)
        {
            throw new NotImplementedException();
        }

        public bool DeleteById(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
