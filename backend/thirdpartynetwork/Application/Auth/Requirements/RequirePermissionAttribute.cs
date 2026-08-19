using Microsoft.AspNetCore.Authorization;

namespace thirdpartynetwork.Application.Auth.Requirements;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    private readonly string _permissionsCsv;
    private bool _isListEndpoint;

    public RequirePermissionAttribute(params string[] permissions)
    {
        _permissionsCsv = string.Join(",", permissions);
        Policy = $"Permission:{_permissionsCsv}";
    }

    public bool IsListEndpoint
    {
        get => _isListEndpoint;
        set
        {
            _isListEndpoint = value;
            Policy = value ? $"Permission:List:{_permissionsCsv}" : $"Permission:{_permissionsCsv}";
        }
    }
}
