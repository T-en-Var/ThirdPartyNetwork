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

            var isAllowed = await CheckPermissionAsync(permission, isAuthenticated);
            
            Console.WriteLine(isAllowed
                ? "[PermissionHandler] Passed"
                : "[PermissionHandler] Refused");

            if (!isAllowed) continue;

            Console.WriteLine("[PermissionHandler] Accepted.");
            context.Succeed(requirement);
            break;
        }

        Console.WriteLine("[PermissionHandler] Complete.");
    }
    
    private async Task<bool> CheckPermissionAsync(string permission, bool isAuthenticated)
    {
        // Phase 1: If not logged in, they ONLY pass if the resource is publicly accessible enough.
        if (!isAuthenticated)
        {
            return Constants.ContentPermissions.Contains(permission) 
                   && await HandleContentRequirementAsync(permission);
        }

        // Phase 2: specific Context Checks (Resource Owner or Content)
        if (Constants.OwnerPermissions.Contains(permission))
        {
            if (await HandleOwnerRequirementAsync(permission)) return true;
        }
        else if (Constants.ContentPermissions.Contains(permission))
        {
            if (await HandleContentRequirementAsync(permission)) return true;
        }

        // Phase 3: Default role checking
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

    private async Task<bool> HandleContentRequirementAsync(string permission)
    {
        string compareString;
        if (Constants.SlugContentPermissions.Contains(permission))
        {
            // Currently, ONLY blog has this.
            // Se we play it easy
            compareString = "slug";
        }
        else
        {
            compareString = permission switch
            {
                Constants.Permissions.ReadFile => "fileId",
                Constants.Permissions.ReadCustomPage => "customPageId",
                Constants.Permissions.ReadPageBlock => "pageBlockId",
                Constants.Permissions.ReadConfiguration => "configurationId",
                _ => throw new Exception("Unknown permission")
            };
        }

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) throw new UnauthorizedHttpException();

        Console.WriteLine($"[PermissionHandler] Checking content permission: {compareString}");

        // In case the request is for getting a list of resources, but not a specific resource.
        // Then we pass the filtering to the service layer to filter per resource.
        if (!httpContext.HasGuidInHttpContextUrl(compareString)) 
            return true;

        var compareValue = httpContext.GetStringFromHttpContextUrl(compareString);

        if (compareValue == null)
        {
            // This now assumes we call a route where we get a list of results, not singular.
            // That is handled by the repository as we check per resource.
            return true;
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

    private async Task<bool> HandleOwnerRequirementAsync(string permission)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) throw new UnauthorizedHttpException();

        var currentUser = await identityService.GetUserProxyAsync();

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