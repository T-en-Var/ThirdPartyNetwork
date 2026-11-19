using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Interfaces.Repositories;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories;

public class ContentViewingLinkRepository(DataContext context) : BaseRepository(context), IContentViewingLinkRepository
{
    public async Task<ContentViewingLink?> GetByIdAsync(Guid id)
    {
        return await context.ContentViewingLinks.FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<ContentViewingLink> CreateAsync(ContentViewingLink link)
    {
        await context.ContentViewingLinks.AddAsync(link);
        await SaveAndUpdateAsync(link);
        return link;
    }

    public async Task MarkUsedAsync(ContentViewingLink link)
    {
        link.UsedAt = DateTime.UtcNow;
        await SaveAndUpdateAsync(link);
    }

    public async Task<int> DeleteExpiredAsync(DateTime now)
    {
        return await context.ContentViewingLinks
            .Where(l => l.ExpiresAt != null && l.ExpiresAt < now)
            .ExecuteDeleteAsync();
    }
}
