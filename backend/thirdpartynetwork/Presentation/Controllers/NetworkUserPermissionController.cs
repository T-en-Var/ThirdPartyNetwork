using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/networks/{networkId:guid}/users/{networkUserId:guid}/permissions")]
[ApiController]
public class NetworkUserPermissionController(INetworkUserPermissionService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkUserPermissionDto>))]
    public IActionResult GetNetworkUserPermissions(Guid networkId, Guid networkUserId)
    {
        var networkUserPermissions = service.GetByNetworkId(networkId, networkUserId);
        return Ok(mapper.Map<IEnumerable<NetworkUserPermissionDto>>(networkUserPermissions));
    }

    [HttpGet("{permissionId:guid}")]
    [ProducesResponseType(200, Type = typeof(NetworkUserPermissionDto))]
    [ProducesResponseType(404)]
    public IActionResult GetNetworkUserPermission(Guid networkId, Guid networkUserId, Guid permissionId)
    {
        var networkUserPermission = service.GetByIds(networkId, networkUserId, permissionId);
        return networkUserPermission != null ? Ok(mapper.Map<NetworkUserPermissionDto>(networkUserPermission)) : NotFound();
    }

    [HttpPost("{permissionId:guid}")]
    [ProducesResponseType(201, Type = typeof(NetworkUserPermissionDto))]
    [ProducesResponseType(404)]
    public IActionResult CreateNetworkUserPermission(Guid networkId, Guid networkUserId, Guid permissionId, 
        CreateNetworkUserPermissionDto createNetworkUserPermission)
    {
        var networkUserPermission = service.Create(networkId, networkUserId, permissionId, createNetworkUserPermission);
        return Ok(mapper.Map<NetworkUserPermissionDto>(networkUserPermission));
    }

    [HttpPut("{permissionId:guid}")]
    [ProducesResponseType(200, Type = typeof(NetworkUserPermissionDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateNetworkUserPermission(Guid networkId, Guid networkUserId, Guid permissionId,
        UpdateNetworkUserPermissionDto updateNetworkUserPermission)
    {
        var networkUserPermission = service.Update(networkId, networkUserId, permissionId, updateNetworkUserPermission);
        return Ok(mapper.Map<NetworkUserPermissionDto>(networkUserPermission));
    }

    [HttpDelete("{permissionId:guid}")]
    [ProducesResponseType(204)]
    public IActionResult DeleteNetworkUserPermission(Guid networkId, Guid networkUserId, Guid permissionId)
    {
        service.DeleteByIds(networkId, networkUserId, permissionId);
        return NoContent();
    }
}
