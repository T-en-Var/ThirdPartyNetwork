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
        return query
            .Include(x => x.Author)
            .ThenInclude(x => x.UserProxy)
            .ThenInclude(x => x.ImageFile)
            .Include(x => x.Author)
            .ThenInclude(x => x.NetworkUserAccesses)
            .ThenInclude(x => x.Access)
            .AsQueryable();
    }

    protected async Task<IQueryable<T>> GetAccessibleQueryableAsync(Guid networkId, bool bypass = false)
    {
        var query = await GetQueryableAsync();

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

    public async Task<ICollection<T>> GetByAuthorIdAsync(Guid networkId, Guid networkUserId, bool bypass = false)
    {
        var query = await GetAccessibleQueryableAsync(networkId, bypass);

        return await query
            .Where(x => x.AuthorId == networkUserId)
            .ToListAsync();
    }
    
    public async Task<ICollection<T>> GetByUserProxyIdAsync(Guid userProxyId)
    {
        var query = await GetQueryableAsync(); 

        query = BaseIncludes(query);
        query = query.Where(x => x.Author.UserProxyId == userProxyId);
    
        return await query.ToListAsync();
    }
}