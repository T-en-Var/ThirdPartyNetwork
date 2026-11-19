using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IRolePermissionService : IBaseService
{
    Task<ICollection<RolePermission>> GetAllAsync();
    Task<ICollection<RolePermission>> GetByRoleIdAsync(Guid networkId, Guid roleId);
    Task<ICollection<RolePermission>> GetByPermissionIdAsync(Guid permissionId);
    Task<RolePermission?> GetByIdsAsync(Guid networkId, Guid roleId, Guid permissionId);
    Task<RolePermission> CreateAsync(Guid networkId, Guid roleId, Guid permissionId, CreateRolePermissionDto createRolePermission);
    Task<RolePermission> UpdateAsync(Guid networkId, Guid roleId, Guid permissionId, UpdateRolePermissionDto updateRolePermission);
    Task DeleteByIdsAsync(Guid networkId, Guid roleId, Guid permissionId);
}
