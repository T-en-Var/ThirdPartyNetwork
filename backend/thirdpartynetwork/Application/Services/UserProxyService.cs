using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Helpers;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class UserProxyService(IUserProxyRepository repository, IPasswordHasher passwordHasher, IMapper mapper) : IUserProxyService
    {
        public ICollection<UserProxy> GetAll()
        {
            var proxies = repository.GetAll();
            return proxies;
        }

        public ICollection<UserProxy> GetByUserId()
        {
            throw new NotImplementedException();
        }

        public UserProxy? GetById(Guid id)
        {
            var proxy = repository.GetById(id);
            return proxy;
        }

        public UserProxy Create(CreateUserProxyDto createUserProxy)
        {
            var proxy = mapper.Map<UserProxy>(createUserProxy);
            proxy.Password = passwordHasher.HashPassword(proxy.Password);
            proxy = repository.Create(proxy);
            return proxy;
        }

        public UserProxy Update(Guid id, UpdateUserProxyDto updateUserProxy)
        {
            if (GetById(id) == null) throw new NotFoundHttpException();
            
            var obj = mapper.Map<UserProxy>(updateUserProxy);
            obj.Id = id;
            obj.Password = passwordHasher.HashPassword(obj.Password);
            
            var proxy = repository.Update(obj);
            return proxy;
        }

        public bool DeleteById(Guid id)
        {
            var proxy = GetById(id);
            return proxy != null && repository.Delete(proxy);
        }
    }
}
