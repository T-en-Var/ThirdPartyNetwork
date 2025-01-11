using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Helpers;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class UserService(IUserRepository repository, IUserProxyService userProxyService, IPasswordHasher passwordHasher, IMapper mapper) : IUserService
    {
        public ICollection<User> GetAll()
        {
            var users = repository.GetAll();
            return users;
        }

        public User? GetById(Guid id)
        {
            var user = repository.GetById(id);
            return user;
        }

        public User Create(CreateUserProxyDto createUserProxy)
        {
            var newUser = new User();
            var user = repository.Create(newUser);

            var proxy = mapper.Map<UserProxy>(createUserProxy);
            proxy.IsDefault = true;
            proxy.Password = passwordHasher.HashPassword(proxy.Password);
            
            user.Proxies.Add(proxy);
            repository.Save();
            
            return user;
        }
        
        public User Update(Guid id, UpdateUserProxyDto updateUserProxy)
        {
            // User ID, not UserProxy ID
            var user = repository.GetById(id);
            if (user == null) throw new NotFoundHttpException();

            var defaultProxy = user.Proxies.FirstOrDefault(proxy => proxy.IsDefault);

            if (defaultProxy == null)
            {
                throw new Exception("User has no Default Proxy, cannot update.");
            }
            
            userProxyService.Update(defaultProxy.Id, updateUserProxy);
            
            return user;
        }

        public bool DeleteById(Guid id)
        {
            var user = GetById(id);
            return user != null && repository.Delete(user);
        }
    }
}
