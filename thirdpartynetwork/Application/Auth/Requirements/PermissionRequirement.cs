using Microsoft.AspNetCore.Authorization;

namespace thirdpartynetwork.Application.Auth.Requirements;

public class PermissionRequirement : IAuthorizationRequirement
{
    public IReadOnlyList<string> Permissions { get; }

    public bool IsListEndpoint { get; }

    public PermissionRequirement(params string[] permissions) : this(false, permissions)
    {
    }

    public PermissionRequirement(bool isListEndpoint, params string[] permissions)
    {
        Permissions = permissions ?? [];
        IsListEndpoint = isListEndpoint;
    }
}
