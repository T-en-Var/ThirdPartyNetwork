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
    IIdentityService identityService,
    IAccessLevelVerifier accessLevelVerifier)
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
                : Constants.ContentPermissions.Contains(permission)
                    ? HandleContentRequirement(permission)
                    : Constants.GeneralPermissions.Contains(permission)
                        ? HandleGeneralRequirement(permission)
                        : HandleRequirement(permission);

            Console.WriteLine(isAllowed
                ? $"[PermissionHandler] Passed"
                : $"[PermissionHandler] Refused");

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

    private bool HandleContentRequirement(string permission)
    {
        var compareString = permission switch
        {
            Constants.Permissions.ReadFile => "fileId",
            Constants.Permissions.ReadCustomPage => "customPageId",
            Constants.Permissions.ReadPageBlock => "pageBlockId",
            _ => throw new Exception("Unknown permission")
        };

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) throw new UnauthorizedHttpException();

        Console.WriteLine($"[PermissionHandler] Checking content permission: {compareString}");
        
        // This now assumes we are GET-ting the list of entity.
        // Those filter their own output based on permissions by themselves.
        if (!httpContext.HasGuidInHttpContextUrl(compareString)) return true;

        var compareId = httpContext.GetGuidFromHttpContextUrl(compareString);
        var networkId = httpContext.GetGuidFromHttpContextUrl("networkId");
        var userProxy = identityService.GetUserProxy();

        if (identityVerifier.IsSuperAdminOrHasSuperPermission(userProxy.Id, permission))
            return true;

        userProxy = userProxyService.GetById(userProxy.Id);
        if (userProxy == null) throw new UnauthorizedHttpException("User does not exist (???)");

        return permission switch
        {
            Constants.Permissions.ReadFile => accessLevelVerifier.CheckFileAccess(compareId, userProxy, networkId),
            Constants.Permissions.ReadCustomPage => accessLevelVerifier.CheckCustomPageAccess(compareId, userProxy, networkId),
            Constants.Permissions.ReadPageBlock => accessLevelVerifier.CheckPageBlockAccess(compareId, userProxy, networkId),
            _ => throw new NotImplementedException("Unknown permission")
        };
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
                httpContext.GetGuidFromHttpContextUrl("userId")),
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
                // If User's Access-Info is incomplete, force them out.
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