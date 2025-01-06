namespace trojoaccounthub.Interfaces.Services
{
    public interface INetworkUserRoleService : IBaseService
    {
        ICollection<NetworkUserRoleDto> GetAll();
        ICollection<NetworkUserRoleDto> GetByNetworkUserId(int id);
        ICollection<NetworkUserRoleDto> GetByRoleId(int id);
        NetworkUserRoleDto? GetByIds(int networkUserId, int roleId);
        NetworkUserRoleDto? Create(int networkId, CreateNetworkUserRoleDto networkUserRole);
        NetworkUserRoleDto? Update(int networkId, int roleId, UpdateNetworkUserRoleDto networkUserRole);
        bool DeleteById(int id);
    }
}
