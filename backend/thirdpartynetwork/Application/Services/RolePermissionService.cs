using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services;

public class RolePermissionService(IRolePermissionRepository repository, IRoleService roleService, IPermissionService permissionService, IMapper mapper) : IRolePermissionService
{
    public ICollection<RolePermission> GetAll()
    {
        var permissions = repository.GetAll();
        return permissions;
    }

    public ICollection<RolePermission> GetByRoleId(Guid networkId, Guid roleId)
    {
        var permissions = repository.GetByRoleId(roleId);
        return permissions;
    }

    public ICollection<RolePermission> GetByPermissionId(Guid permissionId)
    {
        var permissions = repository.GetByPermissionId(permissionId);
        return permissions;
    }

    public RolePermission? GetByIds(Guid networkId, Guid roleId, Guid permissionId)
    {
        var permission = repository.GetByIds(roleId, permissionId);
        return permission;
    }

    public RolePermission Create(Guid networkId, Guid roleId, Guid permissionId, CreateRolePermissionDto createRolePermission)
    {
        if (roleService.GetById(networkId, roleId) == null) throw new NotFoundHttpException("Role not found.");
        if (permissionService.GetById(permissionId) != null) throw new NotFoundHttpException("Permission not found.");
        
        if (repository.Exists(roleId, permissionId)) throw new ConflictHttpException("Already exists.");
        
        var rolePermission = mapper.Map<RolePermission>(createRolePermission);
        rolePermission.RoleId = roleId;
        rolePermission.PermissionId = permissionId;
        
        rolePermission = repository.Create(rolePermission);
        return rolePermission;
    }

    public RolePermission Update(Guid networkId, Guid roleId, Guid permissionId, UpdateRolePermissionDto updateRolePermission)
    {
        if (roleService.GetById(networkId, roleId) == null) throw new NotFoundHttpException("Role not found.");
        if (permissionService.GetById(permissionId) == null) throw new NotFoundHttpException("Permission not found.");
        
        if (!repository.Exists(roleId, permissionId)) throw new NotFoundHttpException();
        
        var rolePermission = mapper.Map<RolePermission>(updateRolePermission);
        rolePermission.RoleId = roleId;
        rolePermission.PermissionId = permissionId;
        
        rolePermission = repository.Update(rolePermission);
        return rolePermission;
    }

    public bool DeleteByIds(Guid networkId, Guid roleId, Guid permissionId)
    {
        var rolePermission = repository.GetByIds(roleId, permissionId);
        return rolePermission != null && repository.Delete(rolePermission);
    }
}
