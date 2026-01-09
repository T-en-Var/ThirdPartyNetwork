using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Helper;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Auth;

public class IdentityService(
    IHttpContextAccessor httpContextAccessor,
    DataContext dataContext) : IIdentityService
{
    public bool IsAuthenticated()
    {
        return _ExtractUserProxyId(true) != Guid.Empty;
    }
    
    public async Task<UserProxy> GetUserProxyAsync(bool omitThrow = false)
    {
        var userProxyId = _ExtractUserProxyId(omitThrow);
        
        var httpContext = GetHttpContext();
        var cacheKey = $"UserProxy_{userProxyId}";
        
        if (httpContext.Items.TryGetValue(cacheKey, out var cached) && cached is UserProxy cachedProxy)
        {
            return cachedProxy;
        }

        var userProxy = await _GetUserProxyAsync(userProxyId);
        if (userProxy == null)
        {
            if (!omitThrow) throw new UnauthorizedHttpException();
            
            userProxy = new UserProxy
            {
                Id = Guid.Empty,
                Username = null,
                FirstName = null,
                LastName = null,
                Email = null,
                Password = null,
                IsDefault = false,
                User = null
            };
        };

        httpContext.Items[cacheKey] = userProxy;
        return userProxy;
    }

    public ClaimsPrincipal? GetClaimsPrincipal()
    {
        var httpContext = httpContextAccessor.HttpContext;
        var appUser = httpContext?.User;

        return appUser;
    }

    private HttpContext GetHttpContext()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) throw new UnauthorizedHttpException();

        return httpContext;
    }

    private async Task<UserProxy?> _GetUserProxyAsync(Guid userProxyId)
    {
        return await dataContext.UserProxies
            .Include(up => up.NetworkUsers)
            .Include(up => up.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.Id == userProxyId);
    }

    private Guid _ExtractUserProxyId(bool omitThrow)
    {
        var httpContext = GetHttpContext();
        var userProxyId = httpContext.GetGuidFromHttpContextUserClaim("uid");

        if (!omitThrow && userProxyId == Guid.Empty) throw new UnauthorizedHttpException();
        return userProxyId;
    }
}