﻿using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IAccessRepository : IBaseRepository
    {
        Task<bool> ExistsAsync(Guid id);
        Task<ICollection<Access>> GetAllAsync();
        Task<Access?> GetByIdAsync(Guid id);
        Task<Access> CreateAsync(Access access);
        Task<Access> UpdateAsync(Access access);
        Task DeleteAsync(Access access);
    }
}
