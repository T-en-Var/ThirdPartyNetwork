using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/auth")]
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
    [Authorize]
    public IActionResult MainNetworkRefresh([FromBody] RefreshTokenDto refreshToken)
    {
        var tokens = service.MainNetworkRefresh(refreshToken);
        return Ok(tokens);
    }
    
    [HttpPost("{networkId:guid}/login")]
    [ProducesResponseType(200, Type = typeof(TokensDto))]
    [ProducesResponseType(401)]
    public IActionResult Login(Guid networkId, [FromBody] LoginDto login)
    {
        var tokens = service.Login(networkId, login);
        return Ok(tokens);
    }

    [HttpPost("{networkId:guid}/refresh")]
    [ProducesResponseType(200, Type = typeof(AccessTokenDto))]
    [ProducesResponseType(401)]
    [Authorize]
    public IActionResult Refresh(Guid networkId, [FromBody] RefreshTokenDto refreshToken)
    {
        var tokens = service.Refresh(networkId, refreshToken);
        return Ok(tokens);
    }
    
    [HttpPost("{networkId:guid}/register")]
    [ProducesResponseType(200, Type = typeof(TokensDto))]
    [ProducesResponseType(401)]
    public IActionResult Register(Guid networkId, [FromBody] CreateUserProxyDto createUserProxy)
    {
        var tokens = service.Register(networkId, createUserProxy);
        return Ok(tokens);
    }
}
