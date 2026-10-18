using thirdpartynetwork.Application.Auth.Requirements;

namespace thirdpartynetwork.Core.Helper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

public class DynamicPermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public DynamicPermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith("Permission:"))
        {
            var body = policyName["Permission:".Length..];

            var isListEndpoint = body.StartsWith("List:");
            if (isListEndpoint) body = body["List:".Length..];

            var isCreateEndpoint = body.StartsWith("Create:");
            if (isCreateEndpoint) body = body["Create:".Length..];

            string? routeKey = null;
            if (body.StartsWith("Route="))
            {
                var routeKeyEnd = body.IndexOf(':');
                routeKey = body[6..routeKeyEnd];
                body = body[(routeKeyEnd + 1)..];
            }

            var permissions = body.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(permissions, isListEndpoint, routeKey, isCreateEndpoint))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        _fallbackPolicyProvider.GetFallbackPolicyAsync();
}