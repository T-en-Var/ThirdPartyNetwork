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
        
        // // Access the header
        // Console.WriteLine("Header claims:");
        // foreach (var header in jwtToken.Header)
        // {
        //     Console.WriteLine($"{header.Key}: {header.Value}");
        // }
        //
        // // Access the payload claims
        // Console.WriteLine("\nPayload claims:");
        // foreach (var claim in jwtToken.Claims)
        // {
        //     Console.WriteLine($"{claim.Type}: {claim.Value}");
        // }
        
        return jwtToken;
    }
    
    public bool ValidateToken(string token)
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
            ClockSkew = TimeSpan.Zero
        };
        
        try
        {
            TokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public bool ValidateTokenPair(ClaimsPrincipal accessToken, string refreshToken)
    {
        Console.WriteLine("AccessToken: " + accessToken);
        Console.WriteLine("RefreshToken: " + refreshToken);
        
        var decodedRefreshToken = DecodeToken(refreshToken);
        
        var accessJti = accessToken.Claims.FirstOrDefault(x => x.Type == "jti");
        var refreshJti = decodedRefreshToken.Claims.FirstOrDefault(x => x.Type == "jti");
        
        if (accessJti == null) return false;
        if (refreshJti == null) return false;
        
        if (accessJti.Value != refreshJti.Value) return false;

        return true;
    }
}