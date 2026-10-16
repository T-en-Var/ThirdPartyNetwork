using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories;

public interface IContentAccessGrantRepository : IBaseRepository
{
    Task<ICollection<ContentAccessGrant>> GetForContentAsync(ContentKind kind, Guid contentId);
    Task<ContentAccessGrant?> GetByIdAsync(Guid id);
    Task<ContentAccessGrant> CreateAsync(ContentAccessGrant grant);
    Task RevokeAsync(ContentAccessGrant grant);
}
