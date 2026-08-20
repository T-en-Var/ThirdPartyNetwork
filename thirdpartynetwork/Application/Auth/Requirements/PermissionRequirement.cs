using Microsoft.AspNetCore.Authorization;

namespace thirdpartynetwork.Application.Auth.Requirements;

public class PermissionRequirement : IAuthorizationRequirement
{
    public IReadOnlyList<string> Permissions { get; }

    /// <summary>
    /// True for content-list endpoints. Set explicitly via 
    /// <see cref="RequirePermissionAttribute.IsListEndpoint"/>. Resolves content endpoints
    /// to true always, letting the querying layer do the filtering per item.
    /// </summary>
    public bool IsListEndpoint { get; }

    /// <summary>
    /// The route parameter that identifies the resource this content permission checks access on. 
    /// Set explicitly via <see cref="RequirePermissionAttribute.RouteKey"/>.
    /// Null (and unused) for non-content permissions and for list endpoints.
    /// </summary>
    public string? RouteKey { get; }

    public PermissionRequirement(params string[] permissions)
        : this(permissions, isListEndpoint: false, routeKey: null)
    {
    }

    public PermissionRequirement(string[] permissions, bool isListEndpoint = false, string? routeKey = null)
    {
        Permissions = permissions ?? [];
        IsListEndpoint = isListEndpoint;
        RouteKey = routeKey;
    }
}
