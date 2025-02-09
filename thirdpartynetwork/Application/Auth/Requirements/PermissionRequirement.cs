using Microsoft.AspNetCore.Authorization;

namespace thirdpartynetwork.Application.Auth.Requirements;

public class PermissionRequirement(params string[] permissions) : IAuthorizationRequirement
{
    public IReadOnlyList<string> Permissions { get; } = permissions ?? [];
}
