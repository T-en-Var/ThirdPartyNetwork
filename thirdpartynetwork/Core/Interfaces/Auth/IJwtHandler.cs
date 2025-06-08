using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace thirdpartynetwork.Core.Interfaces.Auth;

public interface IJwtHandler
{
    JwtSecurityToken DecodeToken(string token);
    bool ValidateToken(string token, bool validateLifetime = true);
    bool ValidateTokenPair(ClaimsPrincipal accessToken, string refreshToken);
}
