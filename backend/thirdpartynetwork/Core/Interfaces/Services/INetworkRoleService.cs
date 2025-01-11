using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface INetworkRoleService : IBaseService
    {
        ICollection<NetworkRole> GetAll();
        ICollection<NetworkRole> GetByNetworkId(Guid id);
        ICollection<NetworkRole> GetByRoleId(Guid id);
        NetworkRole? GetByIds(Guid networkId, Guid roleId);
        NetworkRole Create(Guid networkId, CreateNetworkRoleDto createNetworkRole);
        NetworkRole Update(Guid networkId, Guid roleId, UpdateNetworkRoleDto updateNetworkRole);
        bool DeleteByIds(Guid networkId, Guid roleId);
    }
}
