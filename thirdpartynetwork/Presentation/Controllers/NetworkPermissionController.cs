using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/networks/{networkId:guid}/permissions")]
public class NetworkPermissionController(INetworkPermissionService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkPermissionDto>))]
    [ProducesResponseType(404)]
    public IActionResult GetNetworkPermissions(Guid networkId)
    {
        var networkPermissions = service.GetByNetworkId(networkId);
        return Ok(mapper.Map<IEnumerable<NetworkPermissionDto>>(networkPermissions));
    }

    [HttpGet("{permissionId:guid}")]
    [ProducesResponseType(200, Type = typeof(NetworkPermissionDto))]
    [ProducesResponseType(404)]
    public IActionResult GetNetworkPermission(Guid networkId, Guid permissionId)
    {
        var networkPermission = service.GetByIds(networkId, permissionId);
        if (networkPermission == null) return NotFound();
        return Ok(mapper.Map<NetworkPermissionDto>(networkPermission));
    }

    [HttpPost("{permissionId:guid}")]
    [ProducesResponseType(201, Type = typeof(NetworkPermissionDto))]
    public IActionResult CreateNetworkPermission(Guid networkId, Guid permissionId, CreateNetworkPermissionDto createNetworkPermission)
    {
        var networkPermission = service.Create(networkId, permissionId, createNetworkPermission);
        return Ok(mapper.Map<NetworkPermissionDto>(networkPermission));
    }

    [HttpPatch("{permissionId:guid}")]
    [ProducesResponseType(200, Type = typeof(NetworkPermissionDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateNetworkPermission(Guid networkId, Guid permissionId,
        UpdateNetworkPermissionDto updateNetworkPermission)
    {
        var networkPermission = service.Update(networkId, permissionId, updateNetworkPermission);
        return Ok(mapper.Map<NetworkPermissionDto>(networkPermission));
    }

    [HttpDelete("{permissionId:guid}")]
    [ProducesResponseType(204)]
    public IActionResult DeleteNetworkPermission(Guid networkId, Guid permissionId)
    {
        service.DeleteByIds(networkId, permissionId);
        return NoContent();
    }
}