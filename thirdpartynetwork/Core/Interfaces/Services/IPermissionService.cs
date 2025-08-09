using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IPermissionService : IBaseService
{
    ICollection<Permission> GetAll();
    Permission? GetById(Guid permissionId);
    Permission? GetByName(string name);
    Permission Create(CreatePermissionDto createPermission);
    Permission Update(Guid permissionId, UpdatePermissionDto updatePermission);
    void DeleteById(Guid permissionId);
}
