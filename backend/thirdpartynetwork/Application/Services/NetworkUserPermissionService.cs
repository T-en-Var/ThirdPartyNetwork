using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkUserPermissionService : INetworkUserPermissionService
    {
        public ICollection<NetworkUserPermission> GetAll()
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkUserPermission> GetByNetworkId(Guid id)
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkUserPermission> GetByPermissionId(Guid id)
        {
            throw new NotImplementedException();
        }

        public NetworkUserPermission? GetByIds(Guid networkUserId, Guid permissionId)
        {
            throw new NotImplementedException();
        }

        public NetworkUserPermission Create(Guid networkId, CreateNetworkUserPermissionDto createNetworkUserPermission)
        {
            throw new NotImplementedException();
        }

        public NetworkUserPermission Update(Guid networkUserId, Guid permissionId, UpdateNetworkUserPermissionDto updateNetworkUserPermission)
        {
            throw new NotImplementedException();
        }

        public bool DeleteById(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
