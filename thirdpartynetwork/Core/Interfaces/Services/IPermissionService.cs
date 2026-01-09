using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IPermissionService : IBaseService
{
    Task<ICollection<Permission>> GetAllAsync();
    Task<Permission?> GetByIdAsync(Guid permissionId);
    Task<Permission?> GetByNameAsync(string name);
    Task<Permission> CreateAsync(CreatePermissionDto createPermission);
    Task<Permission> UpdateAsync(Guid permissionId, UpdatePermissionDto updatePermission);
    Task DeleteByIdAsync(Guid permissionId);
}
