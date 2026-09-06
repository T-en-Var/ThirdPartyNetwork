using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories;

public interface IUserContentBaseRepository<T> : IBaseRepository
    where T : UserContentBaseModel
{
    Task<ICollection<T>> GetByNetworkIdAsync(Guid networkId);
    Task<ICollection<T>> GetByAuthorIdAsync(Guid networkId, Guid authorUserProxyId, bool bypass = false);
    Task<ICollection<T>> GetByUserProxyIdAsync(Guid userProxyId);
    Task<T?> GetByIdBypassingAccessAsync(Guid id);
    Task SoftDeleteAsync(T entity);
    Task RestoreAsync(T entity);
}
