using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/networks/{networkId:guid}/users/{networkUserId:guid}/accesses")]
[ApiController]
public class NetworkUserAccessController(INetworkUserAccessService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadAccess)]
    [RequirePermission(Constants.Permissions.ReadUser)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkUserAccessDto>))]
    public IActionResult GetNetworkUserAccesses(Guid networkId, Guid networkUserId)
    {
        var networkUserAccesses = service.GetByNetworkId(networkId, networkUserId);
        return Ok(mapper.Map<IEnumerable<NetworkUserAccessDto>>(networkUserAccesses));
    }

    [HttpGet("{accessId:guid}")]
    [RequirePermission(Constants.Permissions.ReadAccess)]
    [RequirePermission(Constants.Permissions.ReadUser)]
    [ProducesResponseType(200, Type = typeof(NetworkUserAccessDto))]
    [ProducesResponseType(404)]
    public IActionResult GetNetworkUserPermission(Guid networkId, Guid networkUserId, Guid accessId)
    {
        var networkUserPermission = service.GetByIds(networkId, networkUserId, accessId);
        return networkUserPermission != null ? Ok(mapper.Map<NetworkUserAccessDto>(networkUserPermission)) : NotFound();
    }

    // [HttpPost("{accessId:guid}")]
    // [RequirePermission(Constants.Permissions.SystemManaged)]
    // [ProducesResponseType(201, Type = typeof(NetworkUserAccessDto))]
    // [ProducesResponseType(404)]
    // public IActionResult CreateNetworkUserPermission(Guid networkId, Guid networkUserId, Guid accessId, 
    //     CreateNetworkUserAccessDto createNetworkUserPermission)
    // {
    //     var networkUserPermission = service.Create(networkId, networkUserId, accessId, createNetworkUserPermission);
    //     return Ok(mapper.Map<NetworkUserAccessDto>(networkUserPermission));
    // }

    [HttpPut("{accessId:guid}")]
    [RequirePermission(Constants.Permissions.SystemManaged)]
    [ProducesResponseType(200, Type = typeof(NetworkUserAccessDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateNetworkUserPermission(Guid networkId, Guid networkUserId, Guid accessId,
        UpdateNetworkUserAccessDto updateNetworkUserPermission)
    {
        var networkUserPermission = service.Update(networkId, networkUserId, accessId, updateNetworkUserPermission);
        return Ok(mapper.Map<NetworkUserAccessDto>(networkUserPermission));
    }

    // [HttpDelete("{accessId:guid}")]
    // [RequirePermission(Constants.Permissions.SystemManaged)]
    // [ProducesResponseType(204)]
    // public IActionResult DeleteNetworkUserPermission(Guid networkId, Guid networkUserId, Guid accessId)
    // {
    //     service.DeleteByIds(networkId, networkUserId, accessId);
    //     return NoContent();
    // }
}
