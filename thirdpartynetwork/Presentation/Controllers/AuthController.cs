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
    public IActionResult CreateNetwork([FromBody] LoginDto login)
    {
        var tokens = service.Login(login);
        return Ok(tokens);
    }

    [HttpPost("token/refresh")]
    [ProducesResponseType(200, Type = typeof(AccessTokenDto))]
    [ProducesResponseType(401)]
    [Authorize]
    public IActionResult TokenRefresh([FromBody] RefreshTokenDto refreshToken)
    {
        // service.TokenRefresh(refreshToken);
        return Ok(service.TokenRefresh(refreshToken));
    }
    
    [HttpPost("refresh/rotate")]
    [ProducesResponseType(200, Type = typeof(TokensDto))]
    [ProducesResponseType(401)]
    public IActionResult RefreshRotate([FromBody] RefreshTokenDto refreshToken)
    {
        return Ok();
    }
    
    [HttpPost("logout")]
    [ProducesResponseType(200, Type = typeof(TokensDto))]
    [ProducesResponseType(401)]
    public IActionResult Logout([FromBody] RefreshTokenDto refreshToken)
    {
        return Ok();
    }
    
    [Authorize(Policy = "NetworkAdminPolicy")]
    [HttpPost("{networkId:guid}/roles/{roleId:guid}")]
    public IActionResult AssignRoleToNetwork(Guid networkId, Guid roleId)
    {
        return Ok($"Role {roleId} assigned to Network {networkId}");
    }
}
