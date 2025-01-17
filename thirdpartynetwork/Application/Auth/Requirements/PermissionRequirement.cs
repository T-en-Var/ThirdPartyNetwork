using Microsoft.AspNetCore.Authorization;

namespace thirdpartynetwork.Application.Auth.Requirements;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
