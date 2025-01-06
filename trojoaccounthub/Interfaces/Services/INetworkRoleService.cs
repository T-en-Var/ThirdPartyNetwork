namespace trojoaccounthub.Interfaces.Services
{
    public interface INetworkRoleService : IBaseService
    {
        ICollection<NetworkRoleDto> GetAll();
        ICollection<NetworkRoleDto> GetByNetworkId(int id);
        ICollection<NetworkRoleDto> GetByRoleId(int id);
        NetworkRoleDto? GetByIds(int networkId, int roleId);
        NetworkRoleDto? Create(int networkId, CreateNetworkRoleDto networkRole);
        NetworkRoleDto? Update(int networkId, int roleId, UpdateNetworkRoleDto networkRole);
        bool DeleteByIds(int networkId, int roleId);
    }
}
