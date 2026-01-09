using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks/{networkId:guid}/accesses")]
public class NetworkAccessController(INetworkAccessService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadAccess)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkAccessDto>))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetNetworkAccesses(Guid networkId)
    {
        var networkAccesses = await service.GetByNetworkIdAsync(networkId);
        return Ok(mapper.Map<IEnumerable<NetworkAccessDto>>(networkAccesses, opts => opts.Items["networkId"] = networkId));
    }

    [HttpGet("{accessId:guid}")]
    [RequirePermission(Constants.Permissions.ManageAccess)]
    [ProducesResponseType(200, Type = typeof(NetworkAccessDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetNetworkAccess(Guid networkId, Guid accessId)
    {
        var networkAccess = await service.GetByIdsAsync(networkId, accessId);
        if (networkAccess == null) return NotFound();
        return Ok(mapper.Map<NetworkAccessDto>(networkAccess, opts => opts.Items["networkId"] = networkId));
    }

    [HttpPost("{accessId:guid}")]
    [RequirePermission(Constants.Permissions.ManageAccess)]
    [ProducesResponseType(201, Type = typeof(NetworkAccessDto))]
    public async Task<IActionResult> CreateNetworkAccess(Guid networkId, Guid accessId, [FromBody] CreateNetworkAccessDto createNetworkAccess)
    {
        var networkAccess = await service.CreateAsync(networkId, accessId, createNetworkAccess);
        return Ok(mapper.Map<NetworkAccessDto>(networkAccess, opts => opts.Items["networkId"] = networkId));
    }

    [HttpPut("{accessId:guid}")]
    [RequirePermission(Constants.Permissions.ManageAccess)]
    [ProducesResponseType(200, Type = typeof(NetworkAccessDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateNetworkAccess(Guid networkId, Guid accessId,
        [FromBody] UpdateNetworkAccessDto updateNetworkAccess)
    {
        var networkAccess = await service.UpdateAsync(networkId, accessId, updateNetworkAccess);
        return Ok(mapper.Map<NetworkAccessDto>(networkAccess, opts => opts.Items["networkId"] = networkId));
    }

    [HttpDelete("{accessId:guid}")]
    [RequirePermission(Constants.Permissions.ManageAccess)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteNetworkAccess(Guid networkId, Guid accessId)
    {
        await service.DeleteByIdsAsync(networkId, accessId);
        return NoContent();
    }
}