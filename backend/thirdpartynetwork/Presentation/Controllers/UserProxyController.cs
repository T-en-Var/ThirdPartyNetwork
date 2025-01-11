using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth;
using thirdpartynetwork.Application.Dtos.Auth;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/networks/{networkId:int}/users")]
[ApiController]
public class UserProxyController : ControllerBase
{
    [HttpPost("{smth:int}")]
    public IActionResult Create(int networkId, int smth)
    {
        var tokenDto = new TokenDto()
        {
            AccessToken = networkId.ToString(),
            RefreshToken = smth.ToString(),
        };

        return Ok(tokenDto);
    }
}
