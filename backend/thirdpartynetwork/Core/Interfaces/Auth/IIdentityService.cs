using System.Security.Claims;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Auth;

public interface IIdentityService
{
    bool IsAuthenticated();
    Task<UserProxy> GetUserProxyAsync(bool omitThrow = false);
    ClaimsPrincipal? GetClaimsPrincipal();
}