using Microsoft.AspNetCore.Authorization;

namespace thirdpartynetwork.Application.Auth.Requirements;

public class NetworkAdminRequirement : IAuthorizationRequirement
{
    public string NetworkIdRouteParameter { get; }

    public NetworkAdminRequirement(string networkIdRouteParameter)
    {
        NetworkIdRouteParameter = networkIdRouteParameter;
    }
}