using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;
using File = thirdpartynetwork.Core.Models.File;

namespace thirdpartynetwork.Application.Repositories.UserContent;

public abstract class UserContentBaseRepository<T>(DataContext context, IAccessLevelVerifier accessLevelVerifier)
    : BaseRepository(context), IUserContentBaseRepository<T>
    where T : UserContentBaseModel
{
    protected abstract Task<IQueryable<T>> GetQueryableAsync();
    protected abstract string ReadPermissionName { get; }

    private static IQueryable<T> BaseIncludes(IQueryable<T> query)
    {
        // Author is now a UserProxy (portable persona, §2.1), not a NetworkUser membership
        // row — so there's no more per-network NetworkUserAccesses/Access chain to include
        // off of it; the author's own profile image is the equivalent load.
        return query
            .Include(x => x.AuthorUserProxy)
            .ThenInclude(x => x!.ImageFile)
            .AsQueryable();
    }

    // Base query excludes soft-deleted rows by default (§2.1) — callers that need to see
    // deleted rows (restore flows) should query DeletedAt explicitly rather than bypass this.
    private static IQueryable<T> ExcludeDeleted(IQueryable<T> query) => query.Where(x => x.DeletedAt == null);

    protected async Task<IQueryable<T>> GetAccessibleQueryableAsync(Guid networkId, bool bypass = false)
    {
        var query = ExcludeDeleted(await GetQueryableAsync());

        if (!bypass)
        {
            var accessPredicate = await accessLevelVerifier
                .GetAccessPredicateAsync<T>(networkId, ReadPermissionName);

            query = query.Where(accessPredicate);
        }

        return BaseIncludes(query);
    }

    public async Task<ICollection<T>> GetByNetworkIdAsync(Guid networkId)
    {
        var query = await GetAccessibleQueryableAsync(networkId);

        query = BaseIncludes(query);
        query = query.Where(x => x.NetworkId == networkId);

        return await query.ToListAsync();
    }

    // Filters by the content's author persona directly (§2.1) — a plain column comparison,
    // no join through NetworkUser needed now that AuthorUserProxyId lives on the row itself.
    public async Task<ICollection<T>> GetByAuthorIdAsync(Guid networkId, Guid authorUserProxyId, bool bypass = false)
    {
        var query = await GetAccessibleQueryableAsync(networkId, bypass);

        return await query
            .Where(x => x.AuthorUserProxyId == authorUserProxyId)
            .ToListAsync();
    }

    public async Task<ICollection<T>> GetByUserProxyIdAsync(Guid userProxyId)
    {
        var query = ExcludeDeleted(await GetQueryableAsync());

        query = BaseIncludes(query);
        query = query.Where(x => x.AuthorUserProxyId == userProxyId);

        return await query.ToListAsync();
    }

    public async Task<T?> GetByIdBypassingAccessAsync(Guid id)
    {
        var query = await GetAccessibleQueryableAsync(Guid.Empty, bypass: true);
        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task SoftDeleteAsync(T entity)
    {
        entity.DeletedAt = DateTime.UtcNow;
        await SaveAndUpdateAsync(entity);
    }

    public async Task RestoreAsync(T entity)
    {
        entity.DeletedAt = null;
        await SaveAndUpdateAsync(entity);
    }
}
