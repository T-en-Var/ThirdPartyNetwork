using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkRoleService : INetworkRoleService
    {
        public ICollection<NetworkRole> GetAll()
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkRole> GetByNetworkId(Guid id)
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkRole> GetByRoleId(Guid id)
        {
            throw new NotImplementedException();
        }

        public NetworkRole? GetByIds(Guid networkId, Guid roleId)
        {
            throw new NotImplementedException();
        }

        public NetworkRole Create(Guid networkId, CreateNetworkRoleDto networkRole)
        {
            throw new NotImplementedException();
        }

        public NetworkRole Update(Guid networkId, Guid roleId, UpdateNetworkRoleDto networkRole)
        {
            throw new NotImplementedException();
        }

        public bool DeleteByIds(Guid networkId, Guid roleId)
        {
            throw new NotImplementedException();
        }
    }
}
