using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Repositories;

/// <summary>
/// PLAN.md §2.4/§6: one repository serves all five content kinds (ContentKind+ContentId,
/// polymorphic) rather than five near-identical ones — same principle as
/// ContentAccessGrant's own table design (§1).
/// </summary>
public interface IContentAccessGrantRepository : IBaseRepository
{
    Task<ICollection<ContentAccessGrant>> GetForContentAsync(ContentKind kind, Guid contentId);
    Task<ContentAccessGrant?> GetByIdAsync(Guid id);
    Task<ContentAccessGrant> CreateAsync(ContentAccessGrant grant);
    Task RevokeAsync(ContentAccessGrant grant);
}
