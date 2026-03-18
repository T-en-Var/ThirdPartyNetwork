using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks/{networkId:guid}/entitlements")]
[ApiController]
public class NetworkEntitlementsController(INetworkService service, IMapper mapper) : ControllerBase
{
    [HttpPut]
    [RequirePermission(Constants.Permissions.ManageNetwork)]
    [ProducesResponseType(200, Type = typeof(NetworkEntitlementDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid networkId, [FromBody] UpdateNetworkEntitlementDto networkEntitlementUpdate)
    {
        var network = await service.UpdateEntitlementAsync(networkId, networkEntitlementUpdate);
        return StatusCode(200, mapper.Map<NetworkEntitlementDto>(network, opts => opts.Items["networkId"] = networkId));
    }
}