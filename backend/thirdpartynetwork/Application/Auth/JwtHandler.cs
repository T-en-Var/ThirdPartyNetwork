using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Auth;

public class JwtHandler : IJwtHandler
{
    private static readonly JwtSecurityTokenHandler TokenHandler = new();
    private static readonly string TokenSecret = Environment.GetEnvironmentVariable("TOKEN_SECRET")!;
    
    public JwtSecurityToken DecodeToken(string token)
    {
        var jwtToken = TokenHandler.ReadJwtToken(token);
        
        return jwtToken;
    }
    
    public bool ValidateToken(string token, bool validateLifetime = true)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(TokenSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            // You can add additional validation as needed:
            // ValidIssuer = "your-issuer",
            // ValidAudience = "your-audience",
            ClockSkew = TimeSpan.Zero,
            ValidateLifetime = validateLifetime, 
        };
        
        try
        {
            TokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateTokenPair(ClaimsPrincipal accessToken, string refreshToken)
    {
        var decodedRefreshToken = DecodeToken(refreshToken);
        
        var accessJti = accessToken.Claims.FirstOrDefault(x => x.Type == "jti");
        var refreshJti = decodedRefreshToken.Claims.FirstOrDefault(x => x.Type == "jti");
        
        if (accessJti == null) return false;
        if (refreshJti == null) return false;
        
        if (accessJti.Value != refreshJti.Value) return false;

        return true;
    }
}