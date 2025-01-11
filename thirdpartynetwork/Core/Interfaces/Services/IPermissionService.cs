using thirdpartynetwork.Application.Dtos.Permission;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IPermissionService : IBaseService
    {
        ICollection<Permission> GetAll();
        Permission? GetById(Guid id);
        Permission Create(CreatePermissionDto createPermission);
        Permission Update(Guid id, UpdatePermissionDto updatePermission);
        bool DeleteById(Guid id);
    }
}
