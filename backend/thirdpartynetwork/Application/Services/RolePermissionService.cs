using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services;

public class RolePermissionService(IRolePermissionRepository repository, IRoleService roleService, IPermissionService permissionService, IMapper mapper) : IRolePermissionService
{
    public async Task<ICollection<RolePermission>> GetAllAsync()
    {
        var permissions = await repository.GetAllAsync();
        return permissions;
    }

    public async Task<ICollection<RolePermission>> GetByRoleIdAsync(Guid networkId, Guid roleId)
    {
        var permissions = await repository.GetByRoleIdAsync(roleId);
        return permissions;
    }

    public async Task<ICollection<RolePermission>> GetByPermissionIdAsync(Guid permissionId)
    {
        var permissions = await repository.GetByPermissionIdAsync(permissionId);
        return permissions;
    }

    public async Task<RolePermission?> GetByIdsAsync(Guid networkId, Guid roleId, Guid permissionId)
    {
        var permission = await repository.GetByIdsAsync(roleId, permissionId);
        return permission;
    }

    public async Task<RolePermission> CreateAsync(Guid networkId, Guid roleId, Guid permissionId, CreateRolePermissionDto createRolePermission)
    {
        if (await roleService.GetByIdAsync(networkId, roleId) == null) throw new NotFoundHttpException("Role not found.");
        if (await permissionService.GetByIdAsync(permissionId) == null) throw new NotFoundHttpException("Permission not found.");
        
        if (await repository.ExistsAsync(roleId, permissionId)) throw new ConflictHttpException("Already exists.");
        
        var rolePermission = mapper.Map<RolePermission>(createRolePermission);
        rolePermission.RoleId = roleId;
        rolePermission.PermissionId = permissionId;
        
        rolePermission = await repository.CreateAsync(rolePermission);
        return rolePermission;
    }

    public async Task<RolePermission> UpdateAsync(Guid networkId, Guid roleId, Guid permissionId, UpdateRolePermissionDto updateRolePermission)
    {
        if (await roleService.GetByIdAsync(networkId, roleId) == null) throw new NotFoundHttpException("Role not found.");
        if (await permissionService.GetByIdAsync(permissionId) == null) throw new NotFoundHttpException("Permission not found.");
        
        if (!await repository.ExistsAsync(roleId, permissionId)) throw new NotFoundHttpException();
        
        var rolePermission = mapper.Map<RolePermission>(updateRolePermission);
        rolePermission.RoleId = roleId;
        rolePermission.PermissionId = permissionId;
        
        rolePermission = await repository.UpdateAsync(rolePermission);
        return rolePermission;
    }

    public async Task DeleteByIdsAsync(Guid networkId, Guid roleId, Guid permissionId)
    {
        var rolePermission = await repository.GetByIdsAsync(roleId, permissionId);
        if (rolePermission != null) await repository.DeleteAsync(rolePermission);
    }
}
