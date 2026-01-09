﻿using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories
{
    public interface IRoleRepository : IBaseRepository
    {
        Task<bool> ExistsAsync(Guid id);
        Task<ICollection<Role>> GetAllAsync();
        Task<ICollection<Role>> GetByNetworkIdAsync(Guid networkId);
        Task<Role?> GetByIdAsync(Guid networkId, Guid roleId);
        Task<Role> CreateAsync(Role role);
        Task<Role> UpdateAsync(Guid networkId, Role role);
        Task DeleteAsync(Role role);
    }
}
