using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using thirdpartynetwork.Dtos.Token;

namespace thirdpartynetwork.Controllers;

public class IdentityController : ControllerBase
{
    private const string TokenSecret = "stringSecretStringSecretStringSecretStringSecretString";
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(24);

    [HttpPost("login")]
    [ProducesResponseType(200, Type = typeof(TokenDto))]
    [ProducesResponseType(401)]
    public IActionResult CreateNetwork([FromBody] LoginDto login)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TokenSecret);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, login.Username),
            new("userid", "some noodle"),
        };

        // foreach (var claimPair in request.CustomClaims)
        // {
        //     var jsonElement = (JsonElement)claimPair;
        //     var valueType = jsonElement.ValueKind switch
        //     {
        //         JsonValueKind.True => ClaimValueTypes.Boolean,
        //         JsonValueKind.False => ClaimValueTypes.Boolean,
        //         JsonValueKind.Number => ClaimValueTypes.Double,
        //         _ => jsonElement.GetString(),
        //     };
        //     
        //     var claim = new Claim(claimPair.Key, claimPair.Value.ToString()!, valueType);
        //     claims.Add(claim);
        // }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(TokenLifetime),
            Issuer = "https://localhost/",
            Audience = "https://localhost/",
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        var jwt = tokenHandler.WriteToken(token);
        var tokenDto = new TokenDto()
        {
            Token = jwt,
        };
        
        // _mapper.Map<TokenDto>

        return Ok(tokenDto);
    }
}