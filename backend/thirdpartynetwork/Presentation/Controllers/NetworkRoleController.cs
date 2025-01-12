using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/networks/{networkId:guid}/roles")]
[ApiController]
public class NetworkRoleController(INetworkRoleService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkRoleDto>))]
    public IActionResult GetNetworkRoles(Guid networkId)
    {
        var networkRoles = service.GetByNetworkId(networkId);
        return Ok(mapper.Map<IEnumerable<NetworkRoleDto>>(networkRoles));
    }

    [HttpGet("{roleId:guid}")]
    [ProducesResponseType(200, Type = typeof(NetworkRoleDto))]
    [ProducesResponseType(404)]
    public IActionResult GetNetworkRole(Guid networkId, Guid roleId)
    {
        var networkRole = service.GetByIds(networkId, roleId);
        return networkRole != null ? Ok(mapper.Map<NetworkRoleDto>(networkRole)) : NotFound();
    }

    [HttpPost("{roleId:guid}")]
    [ProducesResponseType(200, Type = typeof(NetworkRoleDto))]
    public IActionResult CreateNetworkRole(Guid networkId, Guid roleId, CreateNetworkRoleDto createNetworkRole)
    {
        var networkRole = service.Create(networkId, roleId, createNetworkRole);
        return StatusCode(201, mapper.Map<NetworkRoleDto>(networkRole));
    }

    [HttpPut("{roleId:guid}")]
    [ProducesResponseType(200, Type = typeof(NetworkRoleDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateNetworkRole(Guid networkId, Guid roleId, UpdateNetworkRoleDto updateNetworkRole)
    {
        var networkRole = service.Update(networkId, roleId, updateNetworkRole);
        return Ok(mapper.Map<NetworkRoleDto>(networkRole));
    }

    [HttpDelete("{roleId:guid}")]
    [ProducesResponseType(204)]
    public IActionResult DeleteNetworkRole(Guid networkId, Guid roleId)
    {
        service.DeleteByIds(networkId, roleId);
        return NoContent();
    }
}
