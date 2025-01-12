using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class NetworkUserPermissionService : INetworkUserPermissionService
    {
        public ICollection<NetworkUserPermission> GetAll()
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkUserPermission> GetByNetworkId(Guid networkId, Guid networkUserId)
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkUserPermission> GetByPermissionId(Guid permissionId)
        {
            throw new NotImplementedException();
        }

        public NetworkUserPermission? GetByIds(Guid networkId, Guid networkUserId, Guid permissionId)
        {
            throw new NotImplementedException();
        }

        public NetworkUserPermission Create(Guid networkId, Guid networkUserId, Guid permissionId, CreateNetworkUserPermissionDto createNetworkUserPermission)
        {
            throw new NotImplementedException();
        }

        public NetworkUserPermission Update(Guid networkId, Guid networkUserId, Guid permissionId, UpdateNetworkUserPermissionDto updateNetworkUserPermission)
        {
            throw new NotImplementedException();
        }

        public bool DeleteByIds(Guid networkId, Guid networkUserId, Guid permissionId)
        {
            throw new NotImplementedException();
        }
    }
}
