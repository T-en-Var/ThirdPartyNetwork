namespace trojoaccounthub.Interfaces.Services
{
    public interface INetworkUserPermissionService : IBaseService
    {
        ICollection<NetworkUserPermissionDto> GetAll();
        ICollection<NetworkUserPermissionDto> GetByNetworkId(int id);
        ICollection<NetworkUserPermissionDto> GetByPermissionId(int id);
        NetworkUserPermissionDto? GetByIds(int networkUserId, int permissionId);
        NetworkUserPermissionDto? Create(int networkId, CreateNetworkUserPermissionDto networkUserPermission);
        NetworkUserPermissionDto? Update(int networkUserId, int permissionId, UpdateNetworkUserPermissionDto networkUserPermission);
        bool DeleteById(int id);
    }
}
