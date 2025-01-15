using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services;

public class PermissionService(IPermissionRepository repository, INetworkService networkService, IMapper mapper) : IPermissionService
{
    public ICollection<Permission> GetAll()
    {
        var permissions = repository.GetAll();
        return permissions;
    }

    public Permission? GetById(Guid permissionId)
    {
        var permission = repository.GetById(permissionId);
        return permission;
    }

    public Permission Create(CreatePermissionDto createPermission)
    {
        var permission = mapper.Map<Permission>(createPermission);

        permission = repository.Create(permission);
        return permission;
    }

    public Permission Update(Guid permissionId, UpdatePermissionDto updatePermission)
    {
        if (!repository.Exists(permissionId)) throw new NotFoundHttpException("Permission not found.");
        
        var permission = mapper.Map<Permission>(updatePermission);
        permission.Id = permissionId;
        
        permission = repository.Update(permission);
        return permission;
    }

    public bool DeleteById(Guid permissionId)
    {
        var permission = repository.GetById(permissionId);
        return permission != null && repository.Delete(permission);
    }
}
