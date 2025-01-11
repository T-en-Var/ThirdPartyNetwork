using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkPermissionService : INetworkPermissionService
    {
        public ICollection<NetworkPermission> GetAll()
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkPermission> GetByNetworkId(Guid id)
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkPermission> GetByPermissionId(Guid id)
        {
            throw new NotImplementedException();
        }

        public NetworkPermission? GetByIds(Guid networkId, Guid permissiondId)
        {
            throw new NotImplementedException();
        }

        public NetworkPermission Create(Guid networkId, CreateNetworkPermissionDto networkPermission)
        {
            throw new NotImplementedException();
        }

        public NetworkPermission Update(Guid networkId, Guid permissiondId, UpdateNetworkDto networkPermission)
        {
            throw new NotImplementedException();
        }

        public bool DeleteByIds(Guid networkId, Guid permissiondId)
        {
            throw new NotImplementedException();
        }
    }
}
