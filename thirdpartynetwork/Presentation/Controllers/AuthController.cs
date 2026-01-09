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
    public async Task<IActionResult> MainNetworkLogin([FromBody] LoginDto login)
    {
        var tokens = await service.MainNetworkLoginAsync(login);
        return Ok(tokens);
    }

    [HttpPost("register")]
    [ProducesResponseType(200, Type = typeof(TokensDto))]
    [ProducesResponseType(401)]
    public async Task<IActionResult> MainNetworkRegister([FromBody] CreateUserProxyDto createUserProxy)
    {
        var tokens = await service.MainNetworkRegisterAsync(createUserProxy);
        return Ok(tokens);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(200, Type = typeof(AccessTokenDto))]
    [ProducesResponseType(401)]
    public async Task<IActionResult> MainNetworkRefresh([FromBody] RefreshTokenDto refreshToken)
    {
        var tokens = await service.MainNetworkRefreshAsync(refreshToken);
        return Ok(tokens);
    }

    [HttpPost("{networkId:guid}/login")]
    [ProducesResponseType(200, Type = typeof(AuthorizationCodeDto))]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromRoute] Guid networkId, [FromBody] LoginDto login,
        [FromQuery] string clientId, [FromQuery] string codeChallenge)
    {
        var tokens = await service.ExternalLoginAsync(networkId, login, clientId, codeChallenge);
        return Ok(tokens);
    }

    [HttpPost("{networkId:guid}/refresh")]
    [ProducesResponseType(200, Type = typeof(TokensDto))]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Refresh(Guid networkId, [FromBody] RefreshTokenDto refreshToken)
    {
        var tokens = await service.RefreshAsync(networkId, refreshToken, true);
        return Ok(tokens);
    }

    [HttpPost("{networkId:guid}/register")]
    [ProducesResponseType(200, Type = typeof(AuthorizationCodeDto))]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Register([FromRoute] Guid networkId, [FromBody] CreateUserProxyDto createUserProxy,
        [FromQuery] string clientId, [FromQuery] string codeChallenge)
    {
        var code = await service.ExternalRegisterAsync(networkId, createUserProxy, clientId, codeChallenge);
        return Ok(code);
    }
    
    [HttpPost("token")]
    [ProducesResponseType(200, Type = typeof(TokensDto))]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Token([FromBody] TokenRequestDto dto)
    {
        var tokens = await service.TokenAsync(dto);
        return Ok(tokens);
    }
}