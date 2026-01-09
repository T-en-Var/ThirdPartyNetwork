using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories.UserContent;

public abstract class UserContentBaseRepository<T>(DataContext context, IAccessLevelVerifier accessLevelVerifier) 
    : BaseRepository(context), IUserContentBaseRepository<T>
    where T : UserContentBaseModel
{
    protected abstract Task<IQueryable<T>> GetQueryableAsync();
    protected abstract string ReadPermissionName { get; }

    protected async Task<IQueryable<T>> GetAccessibleQueryableAsync(Guid networkId, bool bypass = false)
    {
        var query = await GetQueryableAsync();

        if (!bypass)
        {
            var accessPredicate = await accessLevelVerifier
                .GetAccessPredicateAsync<T>(networkId, ReadPermissionName);

            query = query.Where(accessPredicate);
        }

        return query
            .Include(x => x.Author)
            .ThenInclude(x => x.UserProxy)
            .AsQueryable();
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

        query = query.Where(x => x.Author.UserProxyId == userProxyId);
    
        return await query.ToListAsync();
    }
}