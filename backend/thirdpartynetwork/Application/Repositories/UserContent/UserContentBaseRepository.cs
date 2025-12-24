using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Repositories.UserContent;

public abstract class UserContentBaseRepository<T>(DataContext context, IAccessLevelVerifier accessLevelVerifier) 
    : BaseRepository(context) 
    where T : UserContentBaseModel
{
    protected abstract IQueryable<T> GetQueryable();

    protected IQueryable<T> GetAccessibleQueryable(Guid networkId, string permissionName, bool bypass = false)
    {
        var query = GetQueryable();

        if (!bypass)
        {
            query = query.Where(accessLevelVerifier.GetAccessPredicate<T>(networkId, permissionName));
        }

        return query
            .Include(x => x.Author)
            .ThenInclude(x => x.UserProxy)
            .AsQueryable();
    }
}