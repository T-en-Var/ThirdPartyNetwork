using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories;

public class RolePermissionRepository(DataContext context) : BaseRepository(context), IRolePermissionRepository
{
    private readonly DataContext _context = context;
    
    public bool Exists(Guid roleId, Guid permissionId)
    {
        return _context.RolePermissions.Any(r => r.RoleId == roleId && r.PermissionId == permissionId);
    }

    public ICollection<RolePermission> GetAll()
    {
        return _context.RolePermissions
            .ToList();
    }

    public ICollection<RolePermission> GetByRoleId(Guid roleId)
    {
        return _context.RolePermissions
            .Where(r => r.RoleId == roleId)
            .ToList();
    }

    public ICollection<RolePermission> GetByPermissionId(Guid permissionId)
    {
        return _context.RolePermissions
            .Where(r => r.PermissionId == permissionId)
            .ToList();
    }

    public RolePermission? GetByIds(Guid roleId, Guid permissionId)
    {
        return _context.RolePermissions
            .FirstOrDefault(r => r.RoleId == roleId && r.PermissionId == permissionId);
    }

    public RolePermission Create(RolePermission rolePermission)
    {
        _context.RolePermissions.Add(rolePermission);
        Save();
        return rolePermission;
    }

    public RolePermission Update(RolePermission rolePermission)
    {
        var existing = GetByIds(rolePermission.RoleId, rolePermission.PermissionId);
        if (existing == null) throw new Exception("RolePermission not found");
            
        _context.Entry(existing).CurrentValues.SetValues(rolePermission);

        Save();
        return existing;
    }

    public bool Delete(RolePermission rolePermission)
    {
        _context.Remove(rolePermission);
        return Save();
    }
}