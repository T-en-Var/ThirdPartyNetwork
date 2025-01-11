using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth;
using thirdpartynetwork.Application.Dtos.Auth;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(TokenGenerator tokenGenerator) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(200, Type = typeof(TokenDto))]
    [ProducesResponseType(401)]
    public IActionResult CreateNetwork([FromBody] LoginDto login)
    {
        var accessToken = tokenGenerator.GenerateAccessToken(
            Guid.NewGuid(), 
            login.Username);
        var refreshToken = tokenGenerator.GenerateRefreshToken();
        
        var tokenDto = new TokenDto()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };

        return Ok(tokenDto);
    }
}
