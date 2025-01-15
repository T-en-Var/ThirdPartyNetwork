using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IPermissionService : IBaseService
{
    ICollection<Permission> GetAll();
    Permission? GetById(Guid permissionId);
    Permission Create(CreatePermissionDto createPermission);
    Permission Update(Guid permissionId, UpdatePermissionDto updatePermission);
    bool DeleteById(Guid permissionId);
}
