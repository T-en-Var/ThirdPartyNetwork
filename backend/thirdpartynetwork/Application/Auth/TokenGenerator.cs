using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Auth;

public class TokenGenerator(IJtiCacheService jtiCacheService, IConfiguration configuration) : ITokenGenerator
{
    private static readonly string TokenSecret = Environment.GetEnvironmentVariable("TOKEN_SECRET")!;
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromHours(3 * 24);

    private class CustomClaim
    {
        public string Key { get; set; }
        public bool Value { get; set; }
    }

    private class NetworkClaim
    {
        public Guid Id { get; set; }
        public List<CustomClaim> Claims { get; set; } = [];
    }

    public TokensDto GenerateTokenPair(
        NetworkUser networkUser,
        string? audience = null,
        Guid? networkId = null,
        string? previousJti = null)
    {
        var jti = Guid.NewGuid();

        var userProxy = networkUser.UserProxy;
        userProxy.ApplyDefaults();

        var tokensDto = new TokensDto()
        {
            AccessToken = GenerateAccessToken(
                jti,
                userProxy.Id,
                userProxy.Username!,
                networkUser.AccessIncomplete,
                audience),
            RefreshToken = GenerateRefreshToken(
                jti,
                userProxy.Id,
                audience,
                previousJti)
        };

        return tokensDto;
    }

    private string GenerateAccessToken(
        Guid jti,
        Guid userId,
        string username,
        bool accessIncomplete,
        string? dynamicAudience = null)
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

        var issuer = configuration["JwtSettings:Issuer"];
        var audience = dynamicAudience ?? configuration["JwtSettings:Audience"];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(AccessTokenLifetime),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken(
        Guid jti,
        Guid userId,
        string? dynamicAudience = null,
        string? previousJti = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TokenSecret);

        jtiCacheService.StoreJti(jti.ToString(), previousJti, RefreshTokenLifetime);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, jti.ToString()),
            new("uid", userId.ToString()),
        };

        var issuer = configuration["JwtSettings:Issuer"];
        var audience = dynamicAudience ?? configuration["JwtSettings:Audience"];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(RefreshTokenLifetime),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);

        return jwt;
    }
}