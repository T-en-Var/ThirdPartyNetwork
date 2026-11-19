using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Interfaces.Repositories;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories;

public class ContentModerationFlagRepository(DataContext context) : BaseRepository(context), IContentModerationFlagRepository
{
    public async Task<ICollection<ContentModerationFlag>> GetForContentAsync(ContentKind kind, Guid contentId)
    {
        return await context.ContentModerationFlags
            .Where(f => f.ContentKind == kind && f.ContentId == contentId)
            .OrderByDescending(f => f.CreatedOn)
            .ToListAsync();
    }

    public async Task<ContentModerationFlag?> GetByIdAsync(Guid id)
    {
        return await context.ContentModerationFlags.FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<ContentModerationFlag> CreateAsync(ContentModerationFlag flag)
    {
        await context.ContentModerationFlags.AddAsync(flag);
        await SaveAndUpdateAsync(flag);
        return flag;
    }

    public async Task<ContentModerationFlag> UpdateAsync(ContentModerationFlag flag)
    {
        await SaveAndUpdateAsync(flag);
        return flag;
    }

    public async Task<bool> HasActiveHideAsync(ContentKind kind, Guid contentId, Guid networkId)
    {
        return await context.ContentModerationFlags.AnyAsync(f =>
            f.ContentKind == kind && f.ContentId == contentId && f.NetworkId == networkId && f.HiddenInNetwork);
    }
}
