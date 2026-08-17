using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories;

public interface IContentViewingLinkRepository : IBaseRepository
{
    Task<ContentViewingLink?> GetByIdAsync(Guid id);
    Task<ContentViewingLink> CreateAsync(ContentViewingLink link);
    Task MarkUsedAsync(ContentViewingLink link);

    /// Cleanup sweep target (§2.3) — expired rows are pure history, delete them outright
    /// rather than soft-revoking (unlike ContentAccessGrant, there's no audit value in
    /// keeping a dead token around).
    Task<int> DeleteExpiredAsync(DateTime now);
}
