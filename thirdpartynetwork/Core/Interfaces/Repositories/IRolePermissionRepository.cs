using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories;

public interface IRolePermissionRepository : IBaseRepository
{
    bool Exists(Guid roleId, Guid permissionId);
    ICollection<RolePermission> GetAll();
    ICollection<RolePermission> GetByRoleId(Guid roleId);
    ICollection<RolePermission> GetByPermissionId(Guid permissionId);
    RolePermission? GetByIds(Guid roleId, Guid permissionId);
    RolePermission Create(RolePermission createRolePermission);
    RolePermission Update(RolePermission updateRolePermission);
    void Delete(RolePermission rolePermission);
}
