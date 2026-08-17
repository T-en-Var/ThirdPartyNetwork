using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Interfaces.Repositories;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories;

public class ContentAccessGrantRepository(DataContext context) : BaseRepository(context), IContentAccessGrantRepository
{
    public async Task<ICollection<ContentAccessGrant>> GetForContentAsync(ContentKind kind, Guid contentId)
    {
        return await context.ContentAccessGrants
            .Where(g => g.ContentKind == kind && g.ContentId == contentId)
            .OrderByDescending(g => g.CreatedOn)
            .ToListAsync();
    }

    public async Task<ContentAccessGrant?> GetByIdAsync(Guid id)
    {
        return await context.ContentAccessGrants.FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<ContentAccessGrant> CreateAsync(ContentAccessGrant grant)
    {
        await context.ContentAccessGrants.AddAsync(grant);
        await SaveAndUpdateAsync(grant);
        return grant;
    }

    public async Task RevokeAsync(ContentAccessGrant grant)
    {
        // Soft revoke (§2.4) — the row (and its history) stays, it's just no longer active.
        grant.RevokedAt = DateTime.UtcNow;
        await SaveAndUpdateAsync(grant);
    }
}
