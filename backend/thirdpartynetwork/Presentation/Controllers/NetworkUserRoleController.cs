using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("/api/networks/{networkId:guid}/users/{networkUserId:guid}/roles")]
[ApiController]
public class NetworkUserRoleController(INetworkUserRoleService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkUserRoleDto>))]
    public IActionResult GetNetworkUserRoles(Guid networkId, Guid networkUserId)
    {
        var networkUserRoles = service.GetByNetworkUserId(networkId, networkUserId);
        return Ok(mapper.Map<IEnumerable<NetworkUserRoleDto>>(networkUserRoles));
    }

    [HttpGet("{roleId:guid}")]
    [ProducesResponseType(200, Type = typeof(NetworkUserRoleDto))]
    [ProducesResponseType(404)]
    public IActionResult GetNetworkUserRole(Guid networkId, Guid userId, Guid roleId)
    {
        var networkUserRole = service.GetByIds(networkId, userId, roleId);
        return networkUserRole != null ? Ok(mapper.Map<NetworkUserRoleDto>(networkUserRole)) : NotFound();
    }

    [HttpPost("{roleId:guid}")]
    [ProducesResponseType(200, Type = typeof(NetworkUserRoleDto))]
    public IActionResult CreateNetworkUserRole(Guid networkId, Guid networkUserId, Guid roleId, 
        CreateNetworkUserRoleDto createNetworkUserRole)
    {
        var networkUserRole = service.Create(networkId, networkUserId, roleId, createNetworkUserRole);
        return Ok(mapper.Map<NetworkUserRoleDto>(networkUserRole));
    }

    [HttpPut("{roleId:guid}")]
    [ProducesResponseType(200, Type = typeof(NetworkUserRoleDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateNetworkUserRole(Guid networkId, Guid networkUserId, Guid roleId,
        UpdateNetworkUserRoleDto updateNetworkUserRole)
    {
        var networkUserRole = service.Update(networkId, networkUserId, roleId, updateNetworkUserRole);
        return Ok(mapper.Map<NetworkUserRoleDto>(networkUserRole));
    }

    [HttpDelete("{roleId:guid}")]
    [ProducesResponseType(204)]
    public IActionResult DeleteNetworkUserRole(Guid networkId, Guid networkUserId, Guid roleId)
    {
        service.DeleteByIds(networkId, networkUserId, roleId);
        return NoContent();
    }
}
