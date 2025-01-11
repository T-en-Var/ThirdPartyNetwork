using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class PermissionService : IPermissionService
    {
        public ICollection<Permission> GetAll()
        {
            throw new NotImplementedException();
        }

        public Permission? GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public Permission Create(CreatePermissionDto permission)
        {
            throw new NotImplementedException();
        }

        public Permission Update(Guid id, UpdatePermissionDto permission)
        {
            throw new NotImplementedException();
        }

        public bool DeleteById(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
