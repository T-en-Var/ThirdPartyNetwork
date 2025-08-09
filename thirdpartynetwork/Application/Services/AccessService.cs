using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class AccessService(IAccessRepository repository, IMapper mapper) : IAccessService
    {
        public ICollection<Access> GetAll()
        {
            var accesses = repository.GetAll();
            return accesses;
        }

        public Access? GetById(Guid id)
        {
            var access = repository.GetById(id);
            return access;
        }

        public Access Create(CreateAccessDto createAccess)
        {
            var access = mapper.Map<Access>(createAccess);
            access = repository.Create(access);
            return access;
        }

        public Access Update(Guid id, UpdateAccessDto updateAccess)
        {
            if (!repository.Exists(id)) throw new NotFoundHttpException();
            
            var access = mapper.Map<Access>(updateAccess);
            access.Id = id;
            
            access = repository.Update(access);
            return access;
        }

        public void DeleteById(Guid id)
        {
            var access = GetById(id);
            if (access != null) repository.Delete(access);
        }
    }
}
