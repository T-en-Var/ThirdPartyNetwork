using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Dtos.Token;
using thirdpartynetwork.Helper;

namespace thirdpartynetwork.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(200, Type = typeof(TokenDto))]
    [ProducesResponseType(401)]
    public IActionResult CreateNetwork([FromBody] LoginDto login)
    {
        var jwt = TokenGenerator.GenerateAccessToken(Guid.NewGuid(), login.Username);
        var tokenDto = new TokenDto()
        {
            AccessToken = jwt,
        };

        return Ok(tokenDto);
    }
}
