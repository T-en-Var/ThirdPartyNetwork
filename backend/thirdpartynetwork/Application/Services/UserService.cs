using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class UserService(IUserRepository repository, IUserProxyService userProxyService, IMapper mapper) : IUserService
    {
        public ICollection<User> GetAll()
        {
            var users = repository.GetAll();
            return mapper.Map<ICollection<User>>(users);
        }

        public User? GetById(Guid id)
        {
            var user = repository.GetById(id);
            return mapper.Map<User>(user);
        }

        public User Create(CreateUserProxyDto userProxy)
        {
            var newUser = new User();
            var user = repository.Create(newUser);

            var proxy = mapper.Map<UserProxy>(userProxy);
            proxy.IsDefault = true;
            
            user.Proxies.Add(proxy);
            repository.Save();
            
            return mapper.Map<User>(user);
        }
        
        public User Update(Guid id, UpdateUserProxyDto userProxy)
        {
            // User ID, not UserProxy ID
            var user = repository.GetById(id);
            if (user == null) throw new NotFoundHttpException();

            var defaultProxy = user.Proxies.FirstOrDefault(proxy => proxy.IsDefault);

            if (defaultProxy == null)
            {
                throw new Exception("User has no Default Proxy, cannot update.");
            }
            
            userProxyService.Update(defaultProxy.Id, userProxy);
            
            return mapper.Map<User>(user);
        }

        public bool DeleteById(Guid id)
        {
            var user = repository.GetById(id);
            return user != null && repository.Delete(user);
        }
    }
}
