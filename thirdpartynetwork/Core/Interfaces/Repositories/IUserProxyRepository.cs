﻿using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IUserProxyRepository : IBaseRepository
    {
        Task<bool> ExistsAsync(Guid id);
        Task<ICollection<UserProxy>> GetAllAsync();
        Task<ICollection<UserProxy>> GetByUserIdAsync(Guid id);
        Task<UserProxy?> GetByIdAsync(Guid id);
        Task<UserProxy?> GetByUsernameAsync(string username);
        Task<UserProxy> CreateAsync(UserProxy userProxy);
        Task SetDefaultUserProxyAsync(Guid userProxyId);
        Task<UserProxy> UpdateAsync(UserProxy userProxy);
        Task<IQueryable<UserProxy>> GetByIdQueryableAsync(Guid id);
        Task DeleteAsync(UserProxy userProxy);
    }
}
