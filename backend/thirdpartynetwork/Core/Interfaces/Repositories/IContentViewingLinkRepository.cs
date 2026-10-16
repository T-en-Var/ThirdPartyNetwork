using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories;

public interface IContentViewingLinkRepository : IBaseRepository
{
    Task<ContentViewingLink?> GetByIdAsync(Guid id);
    Task<ContentViewingLink> CreateAsync(ContentViewingLink link);
    Task MarkUsedAsync(ContentViewingLink link);
    Task<int> DeleteExpiredAsync(DateTime now);
}
