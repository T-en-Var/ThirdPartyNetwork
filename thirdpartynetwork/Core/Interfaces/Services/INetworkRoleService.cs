using thirdpartynetwork.Dtos.NetworkRole;

namespace thirdpartynetwork.Interfaces.Services
{
    public interface INetworkRoleService : IBaseService
    {
        ICollection<NetworkRoleDto> GetAll();
        ICollection<NetworkRoleDto> GetByNetworkId(Guid id);
        ICollection<NetworkRoleDto> GetByRoleId(Guid id);
        NetworkRoleDto? GetByIds(Guid networkId, Guid roleId);
        NetworkRoleDto? Create(Guid networkId, CreateNetworkRoleDto networkRole);
        NetworkRoleDto? Update(Guid networkId, Guid roleId, UpdateNetworkRoleDto networkRole);
        bool DeleteByIds(Guid networkId, Guid roleId);
    }
}
