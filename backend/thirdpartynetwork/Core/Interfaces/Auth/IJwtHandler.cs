using System.IdentityModel.Tokens.Jwt;

namespace thirdpartynetwork.Core.Interfaces.Auth;

public interface IJwtHandler
{
    JwtSecurityToken DecodeToken(string token);
    bool ValidateToken(string token);
}
