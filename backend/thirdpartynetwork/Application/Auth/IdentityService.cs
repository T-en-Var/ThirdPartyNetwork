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
    
    public UserProxy GetUserProxy(bool omitThrow = false)
    {
        var userProxyId = _ExtractUserProxyId(omitThrow);
        
        Console.WriteLine($"userProxyId: {userProxyId}");
        
        var httpContext = GetHttpContext();
        var cacheKey = $"UserProxy_{userProxyId}";
        
        if (httpContext.Items.TryGetValue(cacheKey, out var cached) && cached is UserProxy cachedProxy)
        {
            return cachedProxy;
        }

        var userProxy = _GetUserProxy(userProxyId);
        if (userProxy == null)
        {
        
            Console.WriteLine($"not found");
            if (!omitThrow) throw new UnauthorizedHttpException();
            
            userProxy = new UserProxy
            {
                Id = Guid.NewGuid(),
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

    private UserProxy? _GetUserProxy(Guid userProxyId)
    {
        return dataContext.UserProxies
            .Include(up => up.NetworkUsers)
            .AsNoTracking()
            .FirstOrDefault(up => up.Id == userProxyId);
    }

    private Guid _ExtractUserProxyId(bool omitThrow)
    {
        var httpContext = GetHttpContext();
        var userProxyId = httpContext.GetGuidFromHttpContextUserClaim("uid");

        if (!omitThrow && userProxyId == Guid.Empty) throw new UnauthorizedHttpException();
        return userProxyId;
    }
}