using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using thirdpartynetwork.Application.Auth;

namespace thirdpartynetwork.Helper;

public class TokenGenerator
{
    private static readonly string TokenSecret = Environment.GetEnvironmentVariable("TOKEN_SECRET")!;
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromHours(24);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromHours(7 * 24);
    
    public static string GenerateAccessToken(Guid userId, string username, Guid? previousJti = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TokenSecret);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, TokenJtiTracker.GenerateJti(previousJti).ToString()),
            new(JwtRegisteredClaimNames.Name, username),
            new("uid", userId.ToString()),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(AccessTokenLifetime),
            Issuer = "https://localhost/",
            Audience = "https://localhost/",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);

        return jwt;
    }

    public static string GenerateRefreshToken(Guid userId, string username, Guid? previousJti = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TokenSecret);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, TokenJtiTracker.GenerateJti(previousJti).ToString()),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(RefreshTokenLifetime),
            Issuer = "https://localhost/",
            Audience = "https://localhost/",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);
        
        return jwt;
    }
}
