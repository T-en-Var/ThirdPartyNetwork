using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace thirdpartynetwork.Helper;

public class TokenGenerator
{
    private static readonly string? TokenSecret = Environment.GetEnvironmentVariable("TOKEN_SECRET");
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(24);
    
    public static string GenerateToken(Guid userId, string username)
    {
        if (TokenSecret.IsNullOrEmpty()) throw new ArgumentNullException(nameof(TokenSecret));
            
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TokenSecret);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Name, username),
            new("userid", "some noodle"),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(TokenLifetime),
            Issuer = "https://localhost/",
            Audience = "https://localhost/",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);
        
        return jwt;
    }
}