using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services;

public class PermissionService(IPermissionRepository repository, IMapper mapper) : IPermissionService
{
    public async Task<ICollection<Permission>> GetAllAsync()
    {
        var permissions = await repository.GetAllAsync();
        return permissions;
    }

    public async Task<Permission?> GetByIdAsync(Guid permissionId)
    {
        var permission = await repository.GetByIdAsync(permissionId);
        return permission;
    }

    public async Task<Permission?> GetByNameAsync(string name)
    {
        var permission = await repository.GetByNameAsync(name);
        return permission;
    }

    public async Task<Permission> CreateAsync(CreatePermissionDto createPermission)
    {
        var permission = mapper.Map<Permission>(createPermission);

        permission = await repository.CreateAsync(permission);
        return permission;
    }

    public async Task<Permission> UpdateAsync(Guid permissionId, UpdatePermissionDto updatePermission)
    {
        if (!await repository.ExistsAsync(permissionId)) throw new NotFoundHttpException("Permission not found.");
        
        var permission = mapper.Map<Permission>(updatePermission);
        permission.Id = permissionId;
        
        permission = await repository.UpdateAsync(permission);
        return permission;
    }

    public async Task DeleteByIdAsync(Guid permissionId)
    {
        var permission = await repository.GetByIdAsync(permissionId);
        if (permission != null) await repository.DeleteAsync(permission);
    }
}
