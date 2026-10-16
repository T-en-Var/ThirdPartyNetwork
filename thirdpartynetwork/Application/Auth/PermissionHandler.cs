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
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        Console.WriteLine("[PermissionHandler] Start.");
        PreChecks();

        var isAuthenticated = identityService.IsAuthenticated();

        foreach (var permission in requirement.Permissions)
        {
            Console.WriteLine($"[PermissionHandler] Checking: {permission}");
            if (permission == Constants.Permissions.SystemManaged)
            {
                break;
            }

            var isAllowed = await CheckPermissionAsync(permission, isAuthenticated, requirement.IsListEndpoint,
                requirement.RouteKey, requirement.IsCreateEndpoint);
            
            Console.WriteLine(isAllowed
                ? "[PermissionHandler] Passed"
                : "[PermissionHandler] Refused");

            // At least one requirement needs to pass
            if (!isAllowed) continue;

            Console.WriteLine("[PermissionHandler] Accepted.");
            context.Succeed(requirement);
            break;
        }

        Console.WriteLine("[PermissionHandler] Complete.");
    }
    
    private async Task<bool> CheckPermissionAsync(string permission, bool isAuthenticated, bool isListEndpoint,
        string? routeKey, bool isCreateEndpoint)
    {
        // If not logged in, they ONLY pass if the resource is publicly accessible enough.
        if (!isAuthenticated)
        {
            return Constants.ContentPermissions.Contains(permission)
                   && await HandleContentRequirementAsync(permission, isListEndpoint, routeKey);
        }

        // specific Context Checks (Resource Owner or Content)
        if (Constants.OwnerPermissions.Contains(permission))
        {
            return await HandleOwnerRequirementAsync(permission);
        }

        if (Constants.ContentPermissions.Contains(permission))
        {
            return await HandleContentRequirementAsync(permission, isListEndpoint, routeKey);
        }

        if (Constants.ContentManagePermissions.Contains(permission))
        {
            return isCreateEndpoint
                ? await HandleRequirementAsync(permission)
                : await HandleContentManageRequirementAsync(permission);
        }

        // role checking
        if (Constants.GeneralPermissions.Contains(permission))
        {
            return await HandleGeneralRequirementAsync(permission);
        }

        return await HandleRequirementAsync(permission);
    }
    
    private async Task<bool> HandleRequirementAsync(string permission)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) throw new UnauthorizedHttpException();
        var userProxy = await identityService.GetUserProxyAsync();

        if (await identityVerifier.IsSuperAdminOrHasSuperPermissionAsync(userProxy.Id, permission))
            return true;

        var networkId = httpContext.GetGuidFromHttpContextUrl("networkId");
        return networkId != Guid.Empty &&
               await identityVerifier.IsNetworkAdminOrHasPermissionAsync(networkId, userProxy.Id, permission);
    }

    private async Task<bool> HandleGeneralRequirementAsync(string permission)
    {
        var userProxy = await identityService.GetUserProxyAsync();

        if (await identityVerifier.IsSuperAdminOrHasSuperPermissionAsync(userProxy.Id, permission))
            return true;

        userProxy = await userProxyService.GetByIdAsync(userProxy.Id);
        if (userProxy == null) throw new UnauthorizedHttpException("User does not exist (???)");

        foreach (var u in userProxy.NetworkUsers)
        {
            if (await identityVerifier.IsNetworkAdminOrHasPermissionAsync(u.NetworkId, userProxy.Id, permission))
                return true;
        }
        return false;
    }

    private async Task<bool> HandleContentRequirementAsync(string permission, bool isListEndpoint, string? routeKey)
    {
        if (isListEndpoint) return true;

        if (routeKey == null)
        {
            throw new Exception(
                $"[PermissionHandler] [RequirePermission({permission})] needs RouteKey set " +
                "(or IsListEndpoint = true if this is a list endpoint).");
        }

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) throw new UnauthorizedHttpException();

        Console.WriteLine($"[PermissionHandler] Checking content permission: {routeKey}");

        var compareValue = httpContext.GetStringFromHttpContextUrl(routeKey);

        if (compareValue == null)
        {
            throw new Exception(
                $"[PermissionHandler] Expected route parameter '{routeKey}' for permission " +
                $"'{permission}', but none was found on this request.");
        }

        var networkId = httpContext.GetGuidFromHttpContextUrl("networkId");
        var userProxy = await identityService.GetUserProxyAsync(true);

        if (await identityVerifier.IsSuperAdminOrHasSuperPermissionAsync(userProxy.Id, permission))
            return true;
        
        return permission switch
        {
            Constants.Permissions.ReadFile =>
                await accessLevelVerifier.CheckFileAccessAsync(compareValue, networkId, permission),

            Constants.Permissions.ReadCustomPage =>
                await accessLevelVerifier.CheckCustomPageAccessAsync(compareValue, networkId, permission),

            Constants.Permissions.ReadPageBlock =>
                await accessLevelVerifier.CheckPageBlockAccessAsync(compareValue, networkId, permission),

            Constants.Permissions.ReadConfiguration =>
                await accessLevelVerifier.CheckConfigurationAccessAsync(compareValue, networkId, permission),
            
            Constants.Permissions.ReadBlog => 
                await accessLevelVerifier.CheckBlogAccessAsync(compareValue, networkId, permission),

            _ => throw new NotImplementedException("Unknown permission")
        };
    }

    private async Task<bool> HandleContentManageRequirementAsync(string permission)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) throw new UnauthorizedHttpException();

        var route = ResolveContentRouteMatch(httpContext);
        if (route == null) return false;

        var contentId = httpContext.GetGuidFromHttpContextUrl(route.Value.CompareString);
        if (contentId == Guid.Empty) return false;

        var currentUser = await identityService.GetUserProxyAsync();

        if (await identityVerifier.IsSuperAdminOrHasSuperPermissionAsync(currentUser.Id, permission))
            return true;

        return await identityVerifier.IsContentOwnerAsync(currentUser.Id, route.Value.Kind, contentId);
    }

    private async Task<bool> HandleOwnerRequirementAsync(string permission)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) throw new UnauthorizedHttpException();

        var currentUser = await identityService.GetUserProxyAsync();

        if (permission == Constants.Permissions.IsContentOwner)
        {
            var route = ResolveContentRouteMatch(httpContext);
            if (route == null) return false;

            return await identityVerifier.IsContentOwnerAsync(currentUser.Id, route.Value.Kind,
                httpContext.GetGuidFromHttpContextUrl(route.Value.CompareString));
        }

        return permission switch
        {
            Constants.Permissions.IsUserOwner => await identityVerifier.IsUserOwnerAsync(currentUser.Id,
                httpContext.GetGuidFromHttpContextUrl("userId")),
            Constants.Permissions.IsNetworkUserOwner => await identityVerifier.IsNetworkUserOwnerAsync(currentUser.Id,
                httpContext.GetGuidFromHttpContextUrl("networkUserId")),
            Constants.Permissions.IsUserProxyOwner => await identityVerifier.IsUserProxyOwnerAsync(currentUser.Id,
                httpContext.GetGuidFromHttpContextUrl("userProxyId")),
            _ => false
        };
    }

    private static (Core.Enum.ContentKind Kind, string CompareString)? ResolveContentRouteMatch(HttpContext httpContext)
    {
        var routeValues = httpContext.Request.RouteValues;

        // pageBlockId before customPageId: PageBlock routes nest under customPages
        // customPages/{customPageId}/pageBlocks/{pageBlockId}
        if (routeValues.ContainsKey("fileId")) return (Core.Enum.ContentKind.File, "fileId");
        if (routeValues.ContainsKey("blogId")) return (Core.Enum.ContentKind.Blog, "blogId");
        if (routeValues.ContainsKey("configurationId")) return (Core.Enum.ContentKind.Configuration, "configurationId");
        if (routeValues.ContainsKey("pageBlockId")) return (Core.Enum.ContentKind.PageBlock, "pageBlockId");
        if (routeValues.ContainsKey("customPageId")) return (Core.Enum.ContentKind.CustomPage, "customPageId");

        return null;
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