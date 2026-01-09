using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("me")]
[ApiController]
public class MeController(IMeService service, IMapper mapper) : ControllerBase
{
    [HttpGet("")]
    [ProducesResponseType(200, Type = typeof(UserProxyDto))]
    [ProducesResponseType(401)]
    [Authorize]
    public async Task<IActionResult> MainNetworkRegister()
    {
        var userProxy = await service.GetMeAsync();
        return Ok(mapper.Map<UserProxyDto>(userProxy));
    }
}
