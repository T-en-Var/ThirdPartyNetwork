using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories;

public interface IPermissionRepository : IBaseRepository
{
    Task<bool> ExistsAsync(Guid id);
    Task<ICollection<Permission>> GetAllAsync();
    Task<Permission?> GetByIdAsync(Guid id);
    Task<Permission?> GetByNameAsync(string permissionName);
    Task<Permission> CreateAsync(Permission permission);
    Task<Permission> UpdateAsync(Permission permission);
    Task DeleteAsync(Permission permission);
}
