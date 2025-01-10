namespace thirdpartynetwork.Application.Services
{
    public class NetworkUserPermissionService : INetworkUserPermissionService
    {
        public ICollection<NetworkUserPermissionDto> GetAll()
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkUserPermissionDto> GetByNetworkId(Guid id)
        {
            throw new NotImplementedException();
        }

        public ICollection<NetworkUserPermissionDto> GetByPermissionId(Guid id)
        {
            throw new NotImplementedException();
        }

        public NetworkUserPermissionDto? GetByIds(Guid networkUserId, Guid permissionId)
        {
            throw new NotImplementedException();
        }

        public NetworkUserPermissionDto? Create(Guid networkId, CreateNetworkUserPermissionDto networkUserPermission)
        {
            throw new NotImplementedException();
        }

        public NetworkUserPermissionDto? Update(Guid networkUserId, Guid permissionId,
            UpdateNetworkUserPermissionDto networkUserPermission)
        {
            throw new NotImplementedException();
        }

        public bool DeleteById(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
