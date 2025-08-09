using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Auth;

public class TokenGenerator(IJtiCacheService jtiCacheService) : ITokenGenerator
{
    private static readonly string TokenSecret = Environment.GetEnvironmentVariable("TOKEN_SECRET")!;
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromHours(3 * 24);

    public TokensDto GenerateTokenPair(NetworkUser networkUser, string? previousJti = null)
    {
        var jti = Guid.NewGuid();
        
        var tokensDto = new TokensDto()
        {
            AccessToken = GenerateAccessToken(jti, networkUser.UserProxyId, networkUser.UserProxy.Username, networkUser.AccessIncomplete),
            RefreshToken = GenerateRefreshToken(jti, networkUser.UserProxyId, previousJti)
        };
        
        return tokensDto;
    }

    private static string GenerateAccessToken(Guid jti, Guid userId, string username, bool accessIncomplete)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TokenSecret);
        
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, jti.ToString()),
            new(JwtRegisteredClaimNames.Name, username),
            new("uid", userId.ToString()),
        };

        if (accessIncomplete)
        {
            claims.Add(new Claim("AccessIncomplete", "true"));
        }

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

    private string GenerateRefreshToken(Guid jti, Guid userId, string? previousJti = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TokenSecret);
        
        jtiCacheService.StoreJti(jti.ToString(), previousJti, RefreshTokenLifetime);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, jti.ToString()),
            new("uid", userId.ToString()),
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
