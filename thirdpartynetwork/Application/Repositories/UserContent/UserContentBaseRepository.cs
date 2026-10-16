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
        query = query
            .Include(x => x.AuthorUserProxy)
            .ThenInclude(x => x!.ImageFile)
            .AsQueryable();

        return query;
    }

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

    public async Task<ICollection<T>> GetByAuthorIdAsync(Guid networkId, Guid authorUserProxyId, bool bypass = false)
    {
        var query = await GetAccessibleQueryableAsync(networkId, bypass);

        return await query
            .Where(x => x.AuthorUserProxyId == authorUserProxyId)
            .ToListAsync();
    }

    public async Task<ICollection<T>> GetByUserProxyIdAsync(Guid userProxyId)
    {
        var query = BaseIncludes(ExcludeDeleted(await GetQueryableAsync()));

        var accessPredicate = await accessLevelVerifier.GetCrossNetworkAccessPredicateAsync<T>(ReadPermissionName);
        query = query.Where(accessPredicate).Where(x => x.AuthorUserProxyId == userProxyId);

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
