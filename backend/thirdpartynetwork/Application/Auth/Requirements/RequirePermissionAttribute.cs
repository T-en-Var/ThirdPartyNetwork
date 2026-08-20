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
    /// Set on content-list endpoints. Declares explicitly that this endpoint has no single
    /// resource to check access on. Filtering happens per-resource in the querying step instead.
    /// </summary>
    public bool IsListEndpoint
    {
        get => _isListEndpoint;
        set { _isListEndpoint = value; RecomputePolicy(); }
    }

    /// <summary>
    /// The route parameter that names this endpoint's resource identifeir. 
    /// Required on every non-list content-permission endpoint.
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
