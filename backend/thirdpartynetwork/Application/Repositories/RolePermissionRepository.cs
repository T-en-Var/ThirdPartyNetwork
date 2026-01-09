using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories;

public class RolePermissionRepository(DataContext context) : BaseRepository(context), IRolePermissionRepository
{
    private readonly DataContext _context = context;

    private IQueryable<RolePermission> GetQueryable()
    {
        var query = _context.RolePermissions.AsQueryable();

        query = query
            .Include(r => r.Permission)
            .Include(rp => rp.Role);
        
        return query;
    }
    
    public async Task<bool> ExistsAsync(Guid roleId, Guid permissionId)
    {
        return await _context.RolePermissions.AnyAsync(r => r.RoleId == roleId && r.PermissionId == permissionId);
    }

    public async Task<ICollection<RolePermission>> GetAllAsync()
    {
        return await GetQueryable()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ICollection<RolePermission>> GetByRoleIdAsync(Guid roleId)
    {
        return await GetQueryable()
            .AsNoTracking()
            .Where(r => r.RoleId == roleId)
            .ToListAsync();
    }

    public async Task<ICollection<RolePermission>> GetByPermissionIdAsync(Guid permissionId)
    {
        return await GetQueryable()
            .AsNoTracking()
            .Where(r => r.PermissionId == permissionId)
            .ToListAsync();
    }

    public async Task<RolePermission?> GetByIdsAsync(Guid roleId, Guid permissionId)
    {
        return await _context.RolePermissions
            .FirstOrDefaultAsync(r => r.RoleId == roleId && r.PermissionId == permissionId);
    }

    public async Task<RolePermission> CreateAsync(RolePermission rolePermission)
    {
        await _context.RolePermissions.AddAsync(rolePermission);
        await SaveAndUpdateAsync(rolePermission);
        return rolePermission;
    }

    public async Task<RolePermission> UpdateAsync(RolePermission rolePermission)
    {
        var existing = await GetByIdsAsync(rolePermission.RoleId, rolePermission.PermissionId);
        if (existing == null) throw new Exception("RolePermission not found");
            
        _context.Entry(existing).CurrentValues.SetValues(rolePermission);

        await SaveAndUpdateAsync(existing);
        return existing;
    }

    public async Task DeleteAsync(RolePermission rolePermission)
    {
        _context.Remove(rolePermission);
        await _context.SaveChangesAsync();
    }
}