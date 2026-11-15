using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("mainNetwork")]
[ApiController]
public class MainNetworkController(INetworkService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkDto>))]
    public async Task<IActionResult> GetMainNetwork()
    {
        var network = await service.GetMainNetworkAsync();
        return Ok(mapper.Map<NetworkDto>(network, opts => opts.Items["networkId"] = network.Id));
    }
    
    [HttpGet("details")]
    [Authorize]
    [RequirePermission(Constants.Permissions.ManageNetwork)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkDto>))]
    public async Task<IActionResult> GetMainNetworkDetails()
    {
        var network = await service.GetMainNetworkDetailsAsync();
        return Ok(mapper.Map<NetworkDto>(network, opts => opts.Items["networkId"] = network.Id));
    }
}
