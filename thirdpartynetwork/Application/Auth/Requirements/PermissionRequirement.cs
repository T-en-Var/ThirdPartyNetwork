using Microsoft.AspNetCore.Authorization;

namespace thirdpartynetwork.Application.Auth.Requirements;

public class PermissionRequirement : IAuthorizationRequirement
{
    public IReadOnlyList<string> Permissions { get; }

    /// <summary>
    /// True for content-list endpoints (e.g. GET without a resource id in the route). Set
    /// explicitly via <see cref="RequirePermissionAttribute.IsListEndpoint"/> — PermissionHandler
    /// no longer infers this from whether the route happens to contain an id.
    /// </summary>
    public bool IsListEndpoint { get; }

    /// <summary>
    /// The route parameter that identifies the resource this content permission checks access on
    /// (e.g. "fileId", "slug"). Set explicitly via <see cref="RequirePermissionAttribute.RouteKey"/>,
    /// declared right next to the route it names — not looked up from a separate table.
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
