using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using thirdpartynetwork.Core.Interfaces.Auth;

namespace thirdpartynetwork.Application.Auth;

public class TokenGenerator(IJtiCacheService jtiCacheService) : ITokenGenerator
{
    private static readonly string TokenSecret = Environment.GetEnvironmentVariable("TOKEN_SECRET")!;
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromHours(7 * 24);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromHours(7 * 24 * 7);

    public TokensDto GenerateTokenPair(Guid userId, string username, string? previousJti = null)
    {
        var jti = Guid.NewGuid();
        
        var tokensDto = new TokensDto()
        {
            AccessToken = GenerateAccessToken(jti, userId, username),
            RefreshToken = GenerateRefreshToken(jti, previousJti)
        };
        
        return tokensDto;
    }

    private static string GenerateAccessToken(Guid jti, Guid userId, string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TokenSecret);
        
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, jti.ToString()),
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

    private string GenerateRefreshToken(Guid jti, string? previousJti = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TokenSecret);
        
        jtiCacheService.StoreJti(jti.ToString(), previousJti, AccessTokenLifetime);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, jti.ToString()),
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
