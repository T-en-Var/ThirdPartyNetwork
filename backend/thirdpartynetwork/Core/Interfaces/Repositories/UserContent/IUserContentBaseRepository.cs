using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories;

public interface IUserContentBaseRepository<T> : IBaseRepository
    where T : UserContentBaseModel
{
    Task<ICollection<T>> GetByNetworkIdAsync(Guid networkId);
    Task<ICollection<T>> GetByAuthorIdAsync(Guid networkId, Guid authorUserProxyId, bool bypass = false);
    Task<ICollection<T>> GetByUserProxyIdAsync(Guid userProxyId);

    // Skips the read-access predicate entirely — for callers that have ALREADY performed
    // their own access check by another route (ContentViewingLink resolution, §2.3;
    // IdentityVerifier's ownership lookups) and just need the raw row plus its Includes.
    Task<T?> GetByIdBypassingAccessAsync(Guid id);

    Task SoftDeleteAsync(T entity);
    Task RestoreAsync(T entity);
}
