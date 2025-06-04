using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Helpers;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class UserProxyService(
        IUserProxyRepository repository,
        IUserService userService,
        IPasswordHasher passwordHasher,
        IMapper mapper) : IUserProxyService
    {
        public ICollection<UserProxy> GetAll()
        {
            var proxies = repository.GetAll();
            return proxies;
        }

        public ICollection<UserProxy> GetByUserId(Guid userId)
        {
            var proxies = repository.GetByUserId(userId);
            return proxies;
        }

        public UserProxy? GetById(Guid userProxyId)
        {
            Console.WriteLine("Getting user proxy with id: " + userProxyId);
            var proxy = repository.GetById(userProxyId);
            Console.WriteLine("Got proxy: " + proxy?.Username);
            return proxy;
        }

        public UserProxy? GetByIds(Guid userId, Guid userProxyId)
        {
            if (userService.GetById(userId) == null) throw new NotFoundHttpException("User not found");
            
            var proxy = repository.GetById(userProxyId);
            return proxy;
        }

        public UserProxy? GetByUsername(string username)
        {
            var proxy = repository.GetByUsername(username);
            return proxy;
        }

        public UserProxy Create(Guid userId, CreateUserProxyDto createUserProxy)
        {
            var user = userService.GetById(userId);
            if (user == null) throw new NotFoundHttpException("User not found");

            var isDefault = user.UserProxies.Count == 0;
                
            var proxy = mapper.Map<UserProxy>(createUserProxy);
            proxy.User = user;
            proxy.Password = passwordHasher.HashPassword(proxy.Password);
            proxy.IsDefault = isDefault;
            
            proxy = repository.Create(proxy);
            return proxy;
        }

        public UserProxy Update(Guid userId, Guid userProxyId, UpdateUserProxyDto updateUserProxy)
        {
            var user = userService.GetById(userId);
            if (user == null) throw new NotFoundHttpException("User not found");
            if (GetByIds(userId, userProxyId) == null) throw new NotFoundHttpException();
            
            var proxy = mapper.Map<UserProxy>(updateUserProxy);
            proxy.Id = userProxyId;
            proxy.User = user;
            proxy.Password = passwordHasher.HashPassword(proxy.Password);
            
            proxy = repository.Update(proxy);
            return proxy;
        }

        public IQueryable<UserProxy> GetByIdQueryable(Guid userProxyId)
        {
            var userProxyQueryable = repository.GetByIdQueryable(userProxyId);
            return userProxyQueryable;
        }

        public bool DeleteByIds(Guid userId, Guid userProxyId)
        {
            var proxy = GetByIds(userId, userProxyId);
            return proxy != null && repository.Delete(proxy);
        }
    }
}
