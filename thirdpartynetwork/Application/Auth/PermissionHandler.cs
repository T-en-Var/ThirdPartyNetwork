using Microsoft.AspNetCore.Authorization;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Helper;
using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Auth;

public class PermissionHandler(
    IHttpContextAccessor httpContextAccessor,
    IRoleVerifier roleVerifier,
    IIdentityVerifier identityVerifier)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) return Task.CompletedTask;
        
        var userIdClaim = context.User.FindFirst("uid");
        if (userIdClaim == null 
            || !Guid.TryParse(userIdClaim.Value, out var userProxyId) 
            || userProxyId == Guid.Empty) 
            return Task.CompletedTask;
        
        var isAllowed = false;
        foreach (var permission in requirement.Permissions)
        {
            if (permission == Constants.Permissions.SystemManaged)
            {
                break;
            }
            
            if (Constants.OwnerPermissions.Contains(permission))
            {
                isAllowed = HandleOwnerRequirement(httpContext, userProxyId, permission);
            }
            else
            {
                isAllowed = HandleRequirement(httpContext, userProxyId, permission);
            }

            if (!isAllowed) continue;
            
            context.Succeed(requirement);
            break;
        }
        
        return Task.CompletedTask;
    }

    private bool HandleRequirement(HttpContext httpContext, Guid userProxyId, string permission)
    {
        if (roleVerifier.IsSuperAdminOrHasSuperPermission(userProxyId, permission))
        {
            return true;
        }
        
        var networkId = CustomParser.GetGuidFromHttpContextUrl(httpContext, "networkId");
        return networkId != Guid.Empty && roleVerifier.IsNetworkAdminOrHasPermission(networkId, userProxyId, permission);
    }

    private bool HandleOwnerRequirement(HttpContext httpContext, Guid userProxyId, string permission)
    {
        if (permission 
            is Constants.Permissions.DeleteOwnContent 
            or Constants.Permissions.ViewOwnContent
            or Constants.Permissions.EditOwnContent)
        {
            var contentId = CustomParser.GetGuidFromHttpContextUrl(httpContext, "contentId");
            return contentId != Guid.Empty && identityVerifier.IsContentOwner(userProxyId, contentId);
        }

        if (permission is Constants.Permissions.UpdateSelf)
        {
            var userId = CustomParser.GetGuidFromHttpContextUrl(httpContext, "userId");
            if (userId != Guid.Empty && identityVerifier.IsUserOwner(userProxyId, userId)) return true;
            
            var userProxyUrlId = CustomParser.GetGuidFromHttpContextUrl(httpContext, "userProxyId");
            if (userProxyUrlId != Guid.Empty && identityVerifier.IsUserProxyOwner(userProxyId, userProxyUrlId)) return true;
            
            var networkUserId = CustomParser.GetGuidFromHttpContextUrl(httpContext, "userProxyId");
            if (networkUserId != Guid.Empty && identityVerifier.IsNetworkUserOwner(userProxyId, networkUserId)) return true;

            return false;
        }

        return false;
    }
}