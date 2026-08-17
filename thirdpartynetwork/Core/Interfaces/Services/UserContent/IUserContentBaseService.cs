using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Services;

public interface IUserContentBaseService<T> : IBaseService where T : UserContentBaseModel
{
    public Task<ICollection<T>> GetByAuthorIdAsync(Guid networkUserId);
    public Task<ICollection<T>> GetByUserProxyIdAsync(Guid userProxyId);

    // PLAN.md §6: replaces the old "set AccessLevel" update path — visibility now moves
    // only through these publish actions plus the grant/link endpoints, never a single
    // enum write. Generic here since every content type gets the same PublishedAt column
    // (§2.1); each controller fetches its own concrete entity first (IsContentOwner-gated)
    // then calls through to this shared mutate+save.
    Task<T> PublishAsync(T entity);
    Task<T> UnpublishAsync(T entity);

    // PLAN.md §2.1/§4: without this, OwnershipMode was write-only from the DB's
    // perspective — exposed read-only on the DTO but with no way to ever become
    // NetworkOwned, which would make half of §4's narrowing (author-or-admin for
    // NetworkOwned content) permanently unreachable. IsContentOwner-gated, same as
    // Publish/Unpublish — the CURRENT owner decides whether to hand control to the network.
    Task<T> SetOwnershipModeAsync(T entity, OwnershipMode mode);
}
