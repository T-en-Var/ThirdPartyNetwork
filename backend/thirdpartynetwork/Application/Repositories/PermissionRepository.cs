using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories;

public class PermissionRepository(DataContext context) : BaseRepository(context), IPermissionRepository
{
    private readonly DataContext _context = context;

    private IQueryable<Permission> GetQueryable()
    {
        var query = _context.Permissions
            
            .AsQueryable();
        
        // I'll skip this for now.
        
        return query;
    }
    
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Permissions.AnyAsync(r => r.Id == id);
    }

    public async Task<ICollection<Permission>> GetAllAsync()
    {
        return await _context.Permissions
            
            .ToListAsync();
    }

    public async Task<Permission?> GetByIdAsync(Guid id)
    {
        return await _context.Permissions
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Permission?> GetByNameAsync(string permissionName)
    {
        return await _context.Permissions
            .FirstOrDefaultAsync(r => r.Name == permissionName);
    }

    public async Task<Permission> CreateAsync(Permission permission)
    {
        await _context.Permissions.AddAsync(permission);
        await SaveAndUpdateAsync(permission);
        return permission;
    }

    public async Task<Permission> UpdateAsync(Permission permission)
    {
        var existing = await GetByIdAsync(permission.Id);
        if (existing == null) throw new Exception("Permission not found");
            
        if (existing.IsSystemProtected) throw new UnauthorizedHttpException("Permission is system protected");

        _context.Entry(existing).CurrentValues.SetValues(permission);

        await SaveAndUpdateAsync(existing);
        return existing;
    }

    public async Task DeleteAsync(Permission permission)
    {
        if (permission.IsSystemProtected) throw new UnauthorizedHttpException("Permission is system protected");

        _context.Remove(permission);
        await _context.SaveChangesAsync();
    }
}
