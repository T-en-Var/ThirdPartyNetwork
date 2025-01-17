using Microsoft.AspNetCore.Authorization;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Helper;
using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Auth;

public class PermissionHandler(
    IHttpContextAccessor httpContextAccessor,
    IRoleVerifier roleVerifier,
    ILogger<PermissionHandler> logger)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (requirement.Permission == Constants.Permissions.SystemManaged) return Task.CompletedTask;
        
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) return Task.CompletedTask;
        
        var userIdClaim = context.User.FindFirst("uid");
        if (userIdClaim == null 
            || !Guid.TryParse(userIdClaim.Value, out var userProxyId) 
            || userProxyId == Guid.Empty) 
            return Task.CompletedTask;

        if (roleVerifier.IsSuperAdminOrHasSuperPermission(userProxyId, requirement.Permission))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        
        var networkId = CustomParser.GetGuidFromHttpContextUrl(httpContext, "networkId");
        if (networkId == Guid.Empty) return Task.CompletedTask;

        if (roleVerifier.IsNetworkAdminOrHasPermission(networkId, userProxyId, requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}