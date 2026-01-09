using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories;

public interface IRolePermissionRepository : IBaseRepository
{
    Task<bool> ExistsAsync(Guid roleId, Guid permissionId);
    Task<ICollection<RolePermission>> GetAllAsync();
    Task<ICollection<RolePermission>> GetByRoleIdAsync(Guid roleId);
    Task<ICollection<RolePermission>> GetByPermissionIdAsync(Guid permissionId);
    Task<RolePermission?> GetByIdsAsync(Guid roleId, Guid permissionId);
    Task<RolePermission> CreateAsync(RolePermission createRolePermission);
    Task<RolePermission> UpdateAsync(RolePermission updateRolePermission);
    Task DeleteAsync(RolePermission rolePermission);
}
