using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("me")]
[ApiController]
public class MeController(IMeService service, INetworkService networkService, IMapper mapper) : ControllerBase
{
    [HttpGet("")]
    [ProducesResponseType(200, Type = typeof(UserProxyDto))]
    [ProducesResponseType(401)]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userProxy = await service.GetMeAsync();
        var mainNetwork = await networkService.GetMainNetworkAsync();
        return Ok(mapper.Map<UserProxyDto>(userProxy, opts => opts.Items["networkId"] = mainNetwork.Id));
    }
    
    [HttpGet("permissions")]
    [ProducesResponseType(200, Type = typeof(ICollection<NetworkPermissionsCollectionDto>))]
    [ProducesResponseType(401)]
    [Authorize]
    public async Task<IActionResult> GetMyPermissions()
    {
        var permissionsCollection = await service.GetMyPermissionsAsync();
        return Ok(permissionsCollection);
    }
}
