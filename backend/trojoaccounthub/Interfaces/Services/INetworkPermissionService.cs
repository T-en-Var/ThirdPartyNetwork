namespace trojoaccounthub.Interfaces.Services
{
    public interface INetworkPermissionService : IBaseService
    {
        ICollection<NetworkPermissionDto> GetAll();
        ICollection<NetworkPermissionDto> GetByNetworkId(int id);
        ICollection<NetworkPermissionDto> GetByPermissionId(int id);
        NetworkPermissionDto? GetByIds(int networkId, int permissiondId);
        NetworkPermissionDto? Create(int networkId, CreateNetworkPermissionDto networkPermission);
        NetworkPermissionDto? Update(int networkId, int permissiondId, UpdateNetworkDto networkPermission);
        bool DeleteByIds(int networkId, int permissiondId);
    }
}
