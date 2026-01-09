﻿using AutoMapper;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Interfaces.Helpers;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Services
{
    public class UserService(IUserRepository repository, IMapper mapper) : IUserService
    {
        public async Task<ICollection<User>> GetAllAsync()
        {
            var users = await repository.GetAllAsync();
            return users;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            var user = await repository.GetByIdAsync(id);
            return user;
        }

        public async Task<User> CreateAsync(CreateUserDto createUser)
        {
            var user = mapper.Map<User>(createUser);
            user = await repository.CreateAsync(user);
            
            return user;
        }
        
        public async Task<User> UpdateAsync(Guid userId, UpdateUserDto updateUser)
        {
            if (!await repository.ExistsAsync(userId)) throw new NotFoundHttpException();
            
            var user = mapper.Map<User>(updateUser);
            user.Id = userId;
            
            user = await repository.UpdateAsync(user);

            // var defaultProxy = user.Proxies.FirstOrDefault(proxy => proxy.IsDefault);
            
            // if (defaultProxy == null)
            // {
            //     throw new Exception("User has no Default Proxy, cannot update.");
            // }
            
            // userProxyService.Update(userId, defaultProxy.Id, updateUserProxy);
            
            return user;
        }

        public async Task DeleteByIdAsync(Guid userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null) await repository.DeleteAsync(user);
        }
    }
}
