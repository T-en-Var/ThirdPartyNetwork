using Microsoft.AspNetCore.Authorization;

namespace thirdpartynetwork.Application.Auth.Requirements;

public class AdminRequirement(string claimType, string claimValue) : IAuthorizationRequirement
{
    public string ClaimType { get; } = claimType;
    public string ClaimValue { get; } = claimValue;
}
