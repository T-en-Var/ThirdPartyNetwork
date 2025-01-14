using Microsoft.AspNetCore.Authorization;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Auth;

public class NetworkAdminHandler(
    IHttpContextAccessor httpContextAccessor,
    IRoleVerifier roleVerifier,
    ILogger<NetworkAdminHandler> logger)
    : AuthorizationHandler<NetworkAdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, NetworkAdminRequirement requirement)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var userIdClaim = context.User.FindFirst("uid");
        
        if (userIdClaim == null) return Task.CompletedTask;

        var isValidUserId = Guid.TryParse(userIdClaim.Value, out var userProxyId);
        if (!isValidUserId || userProxyId == Guid.Empty) return Task.CompletedTask;
        
        logger.LogError($"Is super admin: {roleVerifier.IsSuperAdmin(userProxyId)}");
        if (roleVerifier.IsSuperAdmin(userProxyId))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        object? networkIdValue = null;
        var networkId = Guid.Empty;

        var hasNetworkId = httpContext?.Request.RouteValues.TryGetValue(requirement.NetworkIdRouteParameter, out networkIdValue) == true;
        var isValidGuid = networkIdValue is string networkIdString && Guid.TryParse(networkIdString, out networkId);

        if (!hasNetworkId || !isValidGuid) return Task.CompletedTask;
        if (networkId == Guid.Empty) return Task.CompletedTask;
        logger.LogError($"Has role: {roleVerifier.VerifyHasRole(networkId, userProxyId, "Admin")}");
        if (!roleVerifier.VerifyHasRole(networkId, userProxyId, "Admin")) return Task.CompletedTask;
        
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
