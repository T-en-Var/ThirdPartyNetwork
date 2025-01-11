using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class RoleService(IRoleRepository repository, IMapper mapper) : IRoleService
    {
        public ICollection<Role> GetAll()
        {
            var roles = repository.GetAll();
            return roles;
        }

        public Role? GetById(Guid id)
        {
            var role = repository.GetById(id);
            return role;
        }

        public Role Create(CreateRoleDto createRole)
        {
            var role = mapper.Map<Role>(createRole);
            role = repository.Create(role);
            return role;
        }

        public Role Update(Guid id, UpdateRoleDto updateRole)
        {
            if (!repository.Exists(id)) throw new NotFoundHttpException();
            
            var obj = mapper.Map<Role>(updateRole);
            obj.Id = id;
            
            var role = repository.Update(obj);
            return role;
        }

        public bool DeleteById(Guid id)
        {
            var role = GetById(id);
            return role != null && repository.Delete(role);
        }
    }
}
