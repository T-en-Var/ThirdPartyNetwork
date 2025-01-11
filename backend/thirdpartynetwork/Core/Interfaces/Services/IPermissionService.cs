using thirdpartynetwork.Application.Dtos.Permission;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IPermissionService : IBaseService
    {
        ICollection<Permission> GetAll();
        Permission? GetById(Guid id);
        Permission Create(CreatePermissionDto permission);
        Permission Update(Guid id, UpdatePermissionDto permission);
        bool DeleteById(Guid id);
    }
}
