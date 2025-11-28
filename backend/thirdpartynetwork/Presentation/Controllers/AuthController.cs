using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("auth")]
[ApiController]
public class AuthController(IAuthService service) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(200, Type = typeof(TokensDto))]
    [ProducesResponseType(401)]
    public IActionResult MainNetworkLogin([FromBody] LoginDto login)
    {
        var tokens = service.MainNetworkLogin(login);
        return Ok(tokens);
    }

    [HttpPost("register")]
    [ProducesResponseType(200, Type = typeof(TokensDto))]
    [ProducesResponseType(401)]
    public IActionResult MainNetworkRegister([FromBody] CreateUserProxyDto createUserProxy)
    {
        var tokens = service.MainNetworkRegister(createUserProxy);
        return Ok(tokens);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(200, Type = typeof(AccessTokenDto))]
    [ProducesResponseType(401)]
    public IActionResult MainNetworkRefresh([FromBody] RefreshTokenDto refreshToken)
    {
        var tokens = service.MainNetworkRefresh(refreshToken);
        return Ok(tokens);
    }

    [HttpPost("{networkId:guid}/login")]
    [ProducesResponseType(200, Type = typeof(AuthorizationCodeDto))]
    [ProducesResponseType(401)]
    public IActionResult Login([FromRoute] Guid networkId, [FromBody] LoginDto login,
        [FromQuery] string clientId, [FromQuery] string codeChallenge)
    {
        var tokens = service.ExternalLogin(networkId, login, clientId, codeChallenge);
        return Ok(tokens);
    }

    [HttpPost("{networkId:guid}/refresh")]
    [ProducesResponseType(200, Type = typeof(TokensDto))]
    [ProducesResponseType(401)]
    public IActionResult Refresh(Guid networkId, [FromBody] RefreshTokenDto refreshToken)
    {
        var tokens = service.Refresh(networkId, refreshToken, true);
        return Ok(tokens);
    }

    [HttpPost("{networkId:guid}/register")]
    [ProducesResponseType(200, Type = typeof(AuthorizationCodeDto))]
    [ProducesResponseType(401)]
    public IActionResult Register([FromRoute] Guid networkId, [FromBody] CreateUserProxyDto createUserProxy,
        [FromQuery] string clientId, [FromQuery] string codeChallenge)
    {
        var code = service.ExternalRegister(networkId, createUserProxy, clientId, codeChallenge);
        return Ok(code);
    }
    
    [HttpPost("token")]
    [ProducesResponseType(200, Type = typeof(TokensDto))]
    [ProducesResponseType(401)]
    public IActionResult Token([FromBody] TokenRequestDto dto)
    {
        var tokens = service.Token(dto);
        return Ok(tokens);
    }
}