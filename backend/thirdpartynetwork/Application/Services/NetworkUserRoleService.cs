using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkUserRoleService : INetworkUserRoleService
    {
        public ICollection<NetworkUserRole> GetAll()
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkUserRole> GetByNetworkUserId(Guid id)
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkUserRole> GetByRoleId(Guid id)
        {
            throw new NotImplementedException();
        }

        public NetworkUserRole? GetByIds(Guid networkUserId, Guid roleId)
        {
            throw new NotImplementedException();
        }

        public NetworkUserRole Create(Guid networkId, CreateNetworkUserRoleDto networkUserRole)
        {
            throw new NotImplementedException();
        }

        public NetworkUserRole Update(Guid networkId, Guid roleId, UpdateNetworkUserRoleDto networkUserRole)
        {
            throw new NotImplementedException();
        }

        public bool DeleteById(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
