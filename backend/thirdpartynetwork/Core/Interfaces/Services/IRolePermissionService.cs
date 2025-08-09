using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IRolePermissionService : IBaseService
{
    ICollection<RolePermission> GetAll();
    ICollection<RolePermission> GetByRoleId(Guid networkId, Guid roleId);
    ICollection<RolePermission> GetByPermissionId(Guid permissionId);
    RolePermission? GetByIds(Guid networkId, Guid roleId, Guid permissionId);
    RolePermission Create(Guid networkId, Guid roleId, Guid permissionId, CreateRolePermissionDto createRolePermission);
    RolePermission Update(Guid networkId, Guid roleId, Guid permissionId, UpdateRolePermissionDto updateRolePermission);
    void DeleteByIds(Guid networkId, Guid roleId, Guid permissionId);
}
