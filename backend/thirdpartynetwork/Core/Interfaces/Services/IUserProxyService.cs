﻿using thirdpartynetwork.Application.Dtos.UserProxy;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services
{
    public interface IUserProxyService : IBaseService
    {
        Task<ICollection<UserProxy>> GetAllAsync();
        Task<ICollection<UserProxy>> GetByUserIdAsync(Guid userId);
        Task<UserProxy?> GetByIdAsync(Guid userProxyId);
        Task<UserProxy?> GetByIdsAsync(Guid userId, Guid userProxyId);
        Task<UserProxy?> GetByUsernameAsync(string username);
        Task<UserProxy> CreateAsync(Guid userId, CreateUserProxyDto createUser);
        Task<UserProxy> UpdateAsync(Guid userId, Guid userProxyId, UpdateUserProxyDto updateUser);
        Task<IQueryable<UserProxy>> GetByIdQueryableAsync(Guid userProxyId);
        Task DeleteByIdsAsync(Guid userId, Guid userProxyId);
        // ICollection<Network> GetNetworksByIds(Guid userId, Guid userProxyId);
    }
}
