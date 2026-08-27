using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories;

public interface IContentModerationFlagRepository : IBaseRepository
{
    Task<ICollection<ContentModerationFlag>> GetForContentAsync(ContentKind kind, Guid contentId);
    Task<ContentModerationFlag?> GetByIdAsync(Guid id);
    Task<ContentModerationFlag> CreateAsync(ContentModerationFlag flag);
    Task<ContentModerationFlag> UpdateAsync(ContentModerationFlag flag);
    Task<bool> HasActiveHideAsync(ContentKind kind, Guid contentId, Guid networkId);
}
