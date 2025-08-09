using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Helpers;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class UserService(IUserRepository repository, IMapper mapper) : IUserService
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

        public User Create(CreateUserDto createUser)
        {
            var user = mapper.Map<User>(createUser);
            user = repository.Create(user);
            
            return user;
        }
        
        public User Update(Guid userId, UpdateUserDto updateUser)
        {
            if (!repository.Exists(userId)) throw new NotFoundHttpException();
            
            var user = mapper.Map<User>(updateUser);
            user.Id = userId;
            
            user = repository.Update(user);

            // var defaultProxy = user.Proxies.FirstOrDefault(proxy => proxy.IsDefault);
            
            // if (defaultProxy == null)
            // {
            //     throw new Exception("User has no Default Proxy, cannot update.");
            // }
            
            // userProxyService.Update(userId, defaultProxy.Id, updateUserProxy);
            
            return user;
        }

        public void DeleteById(Guid userId)
        {
            var user = GetById(userId);
            if (user != null) repository.Delete(user);
        }
    }
}
