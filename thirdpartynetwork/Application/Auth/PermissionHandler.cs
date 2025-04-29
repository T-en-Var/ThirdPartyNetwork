using Microsoft.AspNetCore.Authorization;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Helper;
using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Auth;

public class PermissionHandler(
    IHttpContextAccessor httpContextAccessor,
    IIdentityVerifier identityVerifier,
    IIdentityService identityService)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        Console.WriteLine("PermissionHandler: ");
        foreach (var permission in requirement.Permissions)
        {
            Console.WriteLine("Checking Permission: " + permission);
            if (permission == Constants.Permissions.SystemManaged)
            {
                break;
            }
            
            var isAllowed = Constants.OwnerPermissions.Contains(permission) 
                ? HandleOwnerRequirement(permission) 
                : HandleRequirement(permission);

            if (!isAllowed) continue;
            
            context.Succeed(requirement);
            break;
        }
        
        return Task.CompletedTask;
    }

    private bool HandleRequirement(string permission)
    {
        Console.WriteLine("HandleRequirement: " + permission);
        
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) throw new UnauthorizedHttpException();
        var userProxy = identityService.GetUserProxy();
        
        if (identityVerifier.IsSuperAdminOrHasSuperPermission(userProxy.Id, permission))
            return true;
        
        var networkId = CustomParser.GetGuidFromHttpContextUrl(httpContext, "networkId");
        return networkId != Guid.Empty && identityVerifier.IsNetworkAdminOrHasPermission(networkId, userProxy.Id, permission);
    }

    private bool HandleOwnerRequirement(string permission)
    {
        Console.WriteLine("HandleOwnerRequirement: " + permission);
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) throw new UnauthorizedHttpException();
        
        var currentUser = identityService.GetUserProxy();
        
        return permission switch
        {
            Constants.Permissions.IsUserOwner => identityVerifier.IsUserOwner(currentUser.Id, CustomParser.GetGuidFromHttpContextUrl(httpContext, "userId")),
            Constants.Permissions.IsNetworkUserOwner => identityVerifier.IsNetworkUserOwner(currentUser.Id, CustomParser.GetGuidFromHttpContextUrl(httpContext, "networkUserId")),
            Constants.Permissions.IsUserProxyOwner => identityVerifier.IsUserProxyOwner(currentUser.Id, CustomParser.GetGuidFromHttpContextUrl(httpContext, "userProxyId")),
            // Constants.Permissions.ReadUser => identityVerifier.IsUserProxyOwner(currentUser.Id, CustomParser.GetGuidFromHttpContextUrl(httpContext, "userProxyId")),
            _ => false
        };
    }
}