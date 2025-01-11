using thirdpartynetwork.Application.Dtos.Role;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IRoleService : IBaseService
    {
        ICollection<Role> GetAll();
        Role? GetById(Guid id);
        Role Create(CreateRoleDto role);
        Role Update(Guid id, UpdateRoleDto role);
        bool DeleteById(Guid id);
    }
}
