using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("auth")]
[ApiController]
public class AuthController(IAuthService service) : ControllerBase
{
    private IActionResult TokensResponse(TokensDto tokens)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Secure = true,
            SameSite = SameSiteMode.None,
        };
        
        Response.Cookies.Append("refreshToken", tokens.RefreshToken, cookieOptions);

        var responseDto = new AccessTokenDto
        {
            AccessToken = tokens.AccessToken,
        };

        return Ok(responseDto);
    }

    private RefreshTokenDto GetRefreshToken()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshTokenStr))
        {
            throw new UnauthorizedHttpException();
        }

        return new RefreshTokenDto() { RefreshToken = refreshTokenStr };
    }

    [HttpPost("login")]
    [ProducesResponseType(200, Type = typeof(AccessTokenDto))]
    [ProducesResponseType(401)]
    public async Task<IActionResult> MainNetworkLogin([FromBody] LoginDto login)
    {
        var tokens = await service.MainNetworkLoginAsync(login);
        return TokensResponse(tokens);
    }

    [HttpPost("register")]
    [ProducesResponseType(200, Type = typeof(AccessTokenDto))]
    [ProducesResponseType(401)]
    public async Task<IActionResult> MainNetworkRegister([FromBody] CreateUserProxyDto createUserProxy)
    {
        var tokens = await service.MainNetworkRegisterAsync(createUserProxy);
        return TokensResponse(tokens);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(200, Type = typeof(AccessTokenDto))]
    [ProducesResponseType(401)]
    public async Task<IActionResult> MainNetworkRefresh()
    {
        var refreshToken = GetRefreshToken();
        var tokens = await service.MainNetworkRefreshAsync(refreshToken);
        return TokensResponse(tokens);
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
    [ProducesResponseType(200, Type = typeof(AccessTokenDto))]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Refresh(Guid networkId)
    {
        var refreshToken = GetRefreshToken();
        var tokens = await service.RefreshAsync(networkId, refreshToken, true);
        return TokensResponse(tokens);
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
    [ProducesResponseType(200, Type = typeof(AccessTokenDto))]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Token([FromBody] TokenRequestDto dto)
    {
        var tokens = await service.TokenAsync(dto);
        return TokensResponse(tokens);
    }
    // testing the git push
}