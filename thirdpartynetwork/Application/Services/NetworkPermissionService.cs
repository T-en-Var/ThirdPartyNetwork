namespace thirdpartynetwork.Application.Services
{
    public class NetworkPermissionService : INetworkPermissionService
    {
        public ICollection<NetworkPermissionDto> GetAll()
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkPermissionDto> GetByNetworkId(Guid id)
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkPermissionDto> GetByPermissionId(Guid id)
        {
            throw new NotImplementedException();
        }

        public NetworkPermissionDto? GetByIds(Guid networkId, Guid permissiondId)
        {
            throw new NotImplementedException();
        }

        public NetworkPermissionDto? Create(Guid networkId, CreateNetworkPermissionDto networkPermission)
        {
            throw new NotImplementedException();
        }

        public NetworkPermissionDto? Update(Guid networkId, Guid permissiondId, UpdateNetworkDto networkPermission)
        {
            throw new NotImplementedException();
        }

        public bool DeleteByIds(Guid networkId, Guid permissiondId)
        {
            throw new NotImplementedException();
        }
    }
}
