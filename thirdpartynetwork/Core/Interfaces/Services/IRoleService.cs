using thirdpartynetwork.Application.Dtos.Role;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IRoleService : IBaseService
    {
        ICollection<Role> GetAll();
        ICollection<Role> GetByNetworkId(Guid networkId);
        Role? GetById(Guid networkId, Guid roleId);
        Role Create(Guid networkId, CreateRoleDto createRole);
        Role Update(Guid networkId, Guid roleId, UpdateRoleDto role);
        bool DeleteById(Guid networkId, Guid roleId);
    }
}
