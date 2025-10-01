using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Application.Auth;

public class TokenGenerator(IJtiCacheService jtiCacheService, IConfiguration configuration) : ITokenGenerator
{
    private static readonly string TokenSecret = Environment.GetEnvironmentVariable("TOKEN_SECRET")!;
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(30);
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

    public TokensDto GenerateTokenPair(NetworkUser networkUser, string? previousJti = null)
    {
        var jti = Guid.NewGuid();

        var networkClaims = (from nu in networkUser.UserProxy.NetworkUsers
            let permissionNames = nu.NetworkUserRoles.SelectMany(r => r.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToList()
            let claimObjects = permissionNames.Select(p => new CustomClaim() { Key = p, Value = true })
                .ToList()
            select new NetworkClaim { Id = nu.NetworkId, Claims = claimObjects }).ToList();

        var userProxy = networkUser.UserProxy.ApplyDefaults();
        
        var tokensDto = new TokensDto()
        {
            AccessToken = GenerateAccessToken(jti, userProxy.Id, userProxy.Username!, networkUser.AccessIncomplete, networkClaims),
            RefreshToken = GenerateRefreshToken(jti, userProxy.Id, previousJti)
        };
        
        return tokensDto;
    }

    private string GenerateAccessToken(Guid jti, Guid userId, string username, bool accessIncomplete, List<NetworkClaim> networkClaims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TokenSecret);
        
        var networksJson = JsonSerializer.Serialize(
            networkClaims.Select(n => new
            {
                id = n.Id,
                claims = n.Claims
            }));
        
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, jti.ToString()),
            new(JwtRegisteredClaimNames.Name, username),
            new("uid", userId.ToString()),
            new("networks", networksJson),
        };

        if (accessIncomplete)
        {
            claims.Add(new Claim("AccessIncomplete", "true"));
        }
        
        var issuer = configuration["JwtSettings:Issuer"];
        var audience = configuration["JwtSettings:Audience"];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(AccessTokenLifetime),
            Issuer = issuer,
            Audience = audience,
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
        
        var issuer = configuration["JwtSettings:Issuer"];
        var audience = configuration["JwtSettings:Audience"];
        
        Console.WriteLine("issuer: " + issuer);
        Console.WriteLine("audience: " + audience);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(RefreshTokenLifetime),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);
        
        return jwt;
    }
}
