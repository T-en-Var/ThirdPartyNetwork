using Microsoft.AspNetCore.Authorization;

namespace thirdpartynetwork.Application.Auth.Requirements;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    private readonly string _permissionsCsv;
    private bool _isListEndpoint;
    private string? _routeKey;

    public RequirePermissionAttribute(params string[] permissions)
    {
        _permissionsCsv = string.Join(",", permissions);
        RecomputePolicy();
    }

    /// <summary>
    /// Set on content-list endpoints (e.g. a GET with no resource id in the route, such as
    /// "GET /networks/{networkId}/files"). Declares explicitly that this endpoint has no single
    /// resource to check access on — filtering happens per-resource in the service layer instead.
    /// This replaces inferring list-vs-single from route shape at request time.
    /// </summary>
    public bool IsListEndpoint
    {
        get => _isListEndpoint;
        set { _isListEndpoint = value; RecomputePolicy(); }
    }

    /// <summary>
    /// The route parameter that names this endpoint's resource id (e.g. "fileId", "blogId",
    /// "slug"). Required on every non-list content-permission endpoint. Declared right here,
    /// next to the actual route template above it — not resolved via a separate permission →
    /// route-key table, since one permission (e.g. ReadBlog) can legitimately mean a different
    /// key on different endpoints (blogId vs. slug).
    /// </summary>
    public string? RouteKey
    {
        get => _routeKey;
        set { _routeKey = value; RecomputePolicy(); }
    }

    private void RecomputePolicy()
    {
        var policy = "Permission:";
        if (_isListEndpoint) policy += "List:";
        if (_routeKey != null) policy += $"Route={_routeKey}:";
        Policy = policy + _permissionsCsv;
    }
}
