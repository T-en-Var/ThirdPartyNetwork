using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using thirdpartynetwork.Core.Interfaces.Auth;

public class JwtHandler : IJwtHandler
{
    private static readonly JwtSecurityTokenHandler TokenHandler = new();
    private static readonly string TokenSecret = Environment.GetEnvironmentVariable("TOKEN_SECRET")!;
    
    // Just decode without verification (to read claims)
    public JwtSecurityToken DecodeToken(string token)
    {
        // Read the token without validating signature
        var jwtToken = TokenHandler.ReadJwtToken(token);
        
        // Access the header
        Console.WriteLine("Header claims:");
        foreach (var header in jwtToken.Header)
        {
            Console.WriteLine($"{header.Key}: {header.Value}");
        }
        
        // Access the payload claims
        Console.WriteLine("\nPayload claims:");
        foreach (var claim in jwtToken.Claims)
        {
            Console.WriteLine($"{claim.Type}: {claim.Value}");
        }
        
        return jwtToken;
    }
    
    // Decode WITH verification (recommended for security)
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
            // This will validate the token and return the principal
            var principal = TokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
            return false;
        }
    }
}