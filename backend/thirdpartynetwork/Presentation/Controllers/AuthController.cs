using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth;
using thirdpartynetwork.Dtos.Token;
using thirdpartynetwork.Helper;

namespace thirdpartynetwork.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly TokenGenerator _tokenGenerator;
    public AuthController(TokenGenerator tokenGenerator)
    {
        _tokenGenerator = tokenGenerator;
    }
    
    [HttpPost("login")]
    [ProducesResponseType(200, Type = typeof(TokenDto))]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateNetwork([FromBody] LoginDto login)
    {
        var accessToken = await _tokenGenerator.GenerateAccessToken(
            Guid.NewGuid(), 
            login.Username);
        var refreshToken = await _tokenGenerator.GenerateRefreshToken();
        
        var tokenDto = new TokenDto()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };

        return Ok(tokenDto);
    }
}
