using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace thirdpartynetwork.Application.Auth;

public class TokenGenerator
{
    private readonly JtiCacheService _jtiCacheService;
    private static readonly string TokenSecret = Environment.GetEnvironmentVariable("TOKEN_SECRET")!;
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromHours(24);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromHours(7 * 24);
    
    public TokenGenerator(JtiCacheService jtiCacheService)
    {
        _jtiCacheService = jtiCacheService;
    }
    
    public async Task<string> GenerateAccessToken(
        Guid userId, 
        string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TokenSecret);
        
        var jti = Guid.NewGuid().ToString();
        
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, jti),
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

    public async Task<string> GenerateRefreshToken(Guid? previousJti = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TokenSecret);

        if (previousJti.HasValue)
        {
            if (!await _jtiCacheService.IsJtiValidAsync(previousJti.Value.ToString()))
            {
                await _jtiCacheService.RemoveJtiAsync(previousJti.Value.ToString());
                throw new UnauthorizedAccessException();
            }
        }
        
        var jti = Guid.NewGuid().ToString();
        await _jtiCacheService.StoreJtiAsync(jti, AccessTokenLifetime);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, jti),
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
