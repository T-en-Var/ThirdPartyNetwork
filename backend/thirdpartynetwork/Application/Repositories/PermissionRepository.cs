using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories;

public class PermissionRepository(DataContext context) : BaseRepository(context), IPermissionRepository
{
    private readonly DataContext _context = context;

    private IQueryable<Permission> GetQueryable()
    {
        var query = _context.Permissions.AsQueryable();
        
        // I'll skip this for now.
        
        return query;
    }
    
    public bool Exists(Guid id)
    {
        return _context.Permissions.Any(r => r.Id == id);
    }

    public ICollection<Permission> GetAll()
    {
        return _context.Permissions
            .ToList();
    }

    public Permission? GetById(Guid id)
    {
        return _context.Permissions
            .FirstOrDefault(r => r.Id == id);
    }

    public Permission? GetByName(string permissionName)
    {
        return _context.Permissions
            .FirstOrDefault(r => r.Name == permissionName);
    }

    public Permission Create(Permission permission)
    {
        _context.Permissions.Add(permission);
        Save();
        return permission;
    }

    public Permission Update(Permission permission)
    {
        var existing = GetById(permission.Id);
        if (existing == null) throw new Exception("Permission not found");
            
        if (existing.IsSystemProtected) throw new UnauthorizedHttpException("Permission is system protected");

        _context.Entry(existing).CurrentValues.SetValues(permission);

        Save();
        return existing;
    }

    public bool Delete(Permission permission)
    {
        if (permission.IsSystemProtected) throw new UnauthorizedHttpException("Permission is system protected");

        _context.Remove(permission);
        return Save();
    }
}
