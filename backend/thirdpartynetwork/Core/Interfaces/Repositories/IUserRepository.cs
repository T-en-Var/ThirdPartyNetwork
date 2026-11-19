﻿using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IUserRepository : IBaseRepository
    {
        Task<bool> ExistsAsync(Guid id);
        Task<ICollection<User>> GetAllAsync();
        Task<ICollection<UserMetricsDto>> GetWithMetricsAsync(Guid? userId = null, Guid? networkId = null);
        Task<User?> GetByIdAsync(Guid id);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task DeleteAsync(User user);
    }
}
