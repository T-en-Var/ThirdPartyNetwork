using Microsoft.AspNetCore.Authorization;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Helper;
using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Auth;

public class PermissionHandler(
    IHttpContextAccessor httpContextAccessor,
    IUserProxyService userProxyService,
    IIdentityVerifier identityVerifier,
    IIdentityService identityService)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        Console.WriteLine("[PermissionHandler] Start.");
        PreChecks();

        foreach (var permission in requirement.Permissions)
        {
            Console.WriteLine($"[PermissionHandler] Checking: {permission}");
            if (permission == Constants.Permissions.SystemManaged)
            {
                break;
            }

            var isAllowed = Constants.OwnerPermissions.Contains(permission)
                ? HandleOwnerRequirement(permission)
                : Constants.GeneralPermissions.Contains(permission)
                    ? HandleGeneralRequirement(permission)
                    : HandleRequirement(permission);

            if (!isAllowed) continue;

            Console.WriteLine("[PermissionHandler] Accepted.");
            context.Succeed(requirement);
            break;
        }

        Console.WriteLine("[PermissionHandler] Complete.");
        return Task.CompletedTask;
    }

    private bool HandleRequirement(string permission)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) throw new UnauthorizedHttpException();
        var userProxy = identityService.GetUserProxy();

        if (identityVerifier.IsSuperAdminOrHasSuperPermission(userProxy.Id, permission))
            return true;

        var networkId = CustomParser.GetGuidFromHttpContextUrl(httpContext, "networkId");
        return networkId != Guid.Empty &&
               identityVerifier.IsNetworkAdminOrHasPermission(networkId, userProxy.Id, permission);
    }

    private bool HandleGeneralRequirement(string permission)
    {
        var userProxy = identityService.GetUserProxy();

        if (identityVerifier.IsSuperAdminOrHasSuperPermission(userProxy.Id, permission))
            return true;

        userProxy = userProxyService.GetById(userProxy.Id);
        if (userProxy == null) throw new UnauthorizedHttpException("User does not exist (???)");

        return userProxy
            .NetworkUsers
            .Any(u => identityVerifier.IsNetworkAdminOrHasPermission(u.NetworkId, userProxy.Id, permission));
    }

    private bool HandleOwnerRequirement(string permission)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) throw new UnauthorizedHttpException();

        var currentUser = identityService.GetUserProxy();

        return permission switch
        {
            Constants.Permissions.IsUserOwner => identityVerifier.IsUserOwner(currentUser.Id,
                CustomParser.GetGuidFromHttpContextUrl(httpContext, "userId")),
            Constants.Permissions.IsNetworkUserOwner => identityVerifier.IsNetworkUserOwner(currentUser.Id,
                CustomParser.GetGuidFromHttpContextUrl(httpContext, "networkUserId")),
            Constants.Permissions.IsUserProxyOwner => identityVerifier.IsUserProxyOwner(currentUser.Id,
                CustomParser.GetGuidFromHttpContextUrl(httpContext, "userProxyId")),
            _ => false
        };
    }

    private void PreChecks()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) throw new UnauthorizedHttpException();
        
        foreach (var claim in httpContext.User.Claims)
        {
            if (claim is { Type: "AccessIncomplete", Value: "true" })
            {
                var routeData = httpContext.GetRouteData();
                var isAccessUpdateRoute =
                    httpContext.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase) &&
                    routeData.Values.ContainsKey("networkId") &&
                    routeData.Values.ContainsKey("networkUserId") &&
                    routeData.Values.ContainsKey("accessId");
                
                // The route `/me` is not checked as it never invokes the PermissionHandler, so automatically passes
                
                if (!isAccessUpdateRoute)
                {
                    throw new UnauthorizedHttpException(UnauthorizedHttpException.BaseMessage, "ACCESS_INCOMPLETE");
                }
            }
        }
    }
}