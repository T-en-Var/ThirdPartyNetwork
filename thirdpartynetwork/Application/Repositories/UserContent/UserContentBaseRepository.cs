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
    protected abstract IQueryable<T> GetQueryable();
    protected abstract string ReadPermissionName { get; }

    protected IQueryable<T> GetAccessibleQueryable(Guid networkId, bool bypass = false)
    {
        var query = GetQueryable();

        if (!bypass)
        {
            query = query.Where(accessLevelVerifier.GetAccessPredicate<T>(networkId, ReadPermissionName));
        }

        return query
            .Include(x => x.Author)
            .ThenInclude(x => x.UserProxy)
            .AsQueryable();
    }

    public ICollection<T> GetByAuthorId(Guid networkId, Guid networkUserId, bool bypass = false)
    {
        return GetAccessibleQueryable(networkId, bypass)
            .Where(x => x.AuthorId == networkUserId)
            .ToList();
    }
    
    public ICollection<T> GetByUserProxyId(Guid userProxyId)
    {
        var query = GetQueryable(); 

        query = query.Where(x => x.Author.UserProxyId == userProxyId);
    
        return query.ToList();
    }
}