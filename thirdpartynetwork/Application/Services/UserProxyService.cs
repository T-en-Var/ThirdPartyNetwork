﻿using AutoMapper;
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
        public async Task<ICollection<UserProxy>> GetAllAsync()
        {
            var proxies = await repository.GetAllAsync();
            proxies = proxies.Select(p => p.ApplyDefaults()).ToList();
            return proxies;
        }

        public async Task<ICollection<UserProxy>> GetByUserIdAsync(Guid userId)
        {
            var proxies = await repository.GetByUserIdAsync(userId);
            proxies = proxies.Select(p => p.ApplyDefaults()).ToList();
            return proxies;
        }

        public async Task<UserProxy?> GetByIdAsync(Guid userProxyId)
        {
            var proxy = await repository.GetByIdAsync(userProxyId);
            proxy = proxy?.ApplyDefaults();
            return proxy;
        }

        public async Task<UserProxy?> GetByIdsAsync(Guid userId, Guid userProxyId)
        {
            if (await userService.GetByIdAsync(userId) == null) throw new NotFoundHttpException("User not found");
            
            var proxy = await repository.GetByIdAsync(userProxyId);
            proxy = proxy?.ApplyDefaults();
            return proxy;
        }

        public async Task<UserProxy?> GetByUsernameAsync(string username)
        {
            var proxy = await repository.GetByUsernameAsync(username);
            proxy = proxy?.ApplyDefaults();
            return proxy;
        }

        public async Task<UserProxy> CreateAsync(Guid userId, CreateUserProxyDto createUserProxy)
        {
            var user = await userService.GetByIdAsync(userId);
            if (user == null) throw new NotFoundHttpException("User not found");

            var isDefault = user.UserProxies.Count == 0;
                
            var proxy = mapper.Map<UserProxy>(createUserProxy);
            proxy.User = user;
            proxy.IsDefault = isDefault;
            
            if (isDefault && proxy.Password == null) throw new BadHttpRequestException("Password is required for default proxy");
            if (proxy.Password != null) proxy.Password = passwordHasher.HashPassword(proxy.Password);
            
            proxy = await repository.CreateAsync(proxy);
            return proxy;
        }

        public async Task<UserProxy> UpdateAsync(Guid userId, Guid userProxyId, UpdateUserProxyDto updateUserProxy)
        {
            var user = await userService.GetByIdAsync(userId);
            if (user == null) throw new NotFoundHttpException("User not found");
            var existing = await GetByIdsAsync(userId, userProxyId);
            if (existing == null) throw new NotFoundHttpException();

            if (updateUserProxy is { KeepPassword: false, IsDefault: true }) throw new BadHttpRequestException("Default proxies require a password");
            
            var proxy = mapper.Map<UserProxy>(updateUserProxy);
            proxy.Id = userProxyId;
            
            if (!updateUserProxy.KeepPassword || updateUserProxy.Password != null)
            {
                proxy.Password = updateUserProxy.Password != null 
                    ? passwordHasher.HashPassword(updateUserProxy.Password) 
                    : null;
            }
            else
            {
                proxy.Password = existing.Password;
            }

            proxy.ImageFileId = updateUserProxy.FileLink?.Id;
            
            proxy = await repository.UpdateAsync(proxy);
            return proxy;
        }

        public async Task<IQueryable<UserProxy>> GetByIdQueryableAsync(Guid userProxyId)
        {
            var userProxyQueryable = await repository.GetByIdQueryableAsync(userProxyId);
            return userProxyQueryable;
        }

        public async Task DeleteByIdsAsync(Guid userId, Guid userProxyId)
        {
            var proxy = await GetByIdsAsync(userId, userProxyId);
            if (proxy != null) await repository.DeleteAsync(proxy);
        }
    }
}
