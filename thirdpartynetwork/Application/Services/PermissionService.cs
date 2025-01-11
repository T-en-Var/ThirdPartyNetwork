using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class PermissionService(IPermissionRepository repository, IMapper mapper) : IPermissionService
    {
        public ICollection<Permission> GetAll()
        {
            var permissions = repository.GetAll();
            return permissions;
        }

        public Permission? GetById(Guid id)
        {
            var permission = repository.GetById(id);
            if (permission == null) throw new NotFoundHttpException();
            return permission;
        }

        public Permission Create(CreatePermissionDto createPermission)
        {
            var permission = repository.Create(mapper.Map<Permission>(createPermission));
            return permission;
        }

        public Permission Update(Guid id, UpdatePermissionDto updatePermission)
        {
            if (!repository.Exists(id)) throw new NotFoundHttpException();
            
            var obj = mapper.Map<Permission>(updatePermission);
            obj.Id = id;
            
            var permission = repository.Update(obj);
            return permission;
        }

        public bool DeleteById(Guid id)
        {
            var permission = GetById(id);
            return permission != null && repository.Delete(permission);
        }
    }
}
