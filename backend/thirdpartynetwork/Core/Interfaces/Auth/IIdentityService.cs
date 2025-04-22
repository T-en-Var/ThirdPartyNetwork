using System.Security.Claims;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Core.Interfaces.Auth;

public interface IIdentityService
{
    UserProxy GetUserProxy(bool omitThrow = false);
    ClaimsPrincipal? GetClaimsPrincipal();
}