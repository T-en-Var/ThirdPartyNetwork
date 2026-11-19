using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks/{networkId:guid}/featureFlags")]
[ApiController]
public class NetworkFeatureFlagsController(INetworkService service, IMapper mapper) : ControllerBase
{
    [HttpPut]
    [RequirePermission(Constants.Permissions.ManageNetwork)]
    [ProducesResponseType(200, Type = typeof(NetworkFeatureFlagDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid networkId, [FromBody] UpdateNetworkFeatureFlagDto networkFeatureFlagUpdate)
    {
        var network = await service.UpdateFeatureFlagAsync(networkId, networkFeatureFlagUpdate);
        return StatusCode(200, mapper.Map<NetworkFeatureFlagDto>(network, opts => opts.Items["networkId"] = networkId));
    }
}