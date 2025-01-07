using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using thirdpartynetwork.Dtos.Token;
using thirdpartynetwork.Helper;

namespace thirdpartynetwork.Controllers;

public class IdentityController : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(200, Type = typeof(TokenDto))]
    [ProducesResponseType(401)]
    public IActionResult CreateNetwork([FromBody] LoginDto login)
    {
        // var tokenGenerator = new TokenGenerator();
        var jwt = TokenGenerator.GenerateToken(Guid.NewGuid(), login.Username);
        var tokenDto = new TokenDto()
        {
            Token = jwt,
        };
        
        // _mapper.Map<TokenDto>

        return Ok(tokenDto);
    }
}