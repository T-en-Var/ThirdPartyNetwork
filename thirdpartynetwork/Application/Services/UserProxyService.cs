using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class UserProxyService(IUserProxyRepository repository, IMapper mapper) : IUserProxyService
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

        public UserProxy Create(CreateUserProxyDto userProxy)
        {
            var proxy = repository.Create(mapper.Map<UserProxy>(userProxy));
            return proxy;
        }

        public UserProxy Update(Guid id, UpdateUserProxyDto userProxy)
        {
            if (GetById(id) == null) throw new NotFoundHttpException();
            var obj = mapper.Map<UserProxy>(userProxy);
            obj.Id = id;
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
