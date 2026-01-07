using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks/{networkId:guid}/users/{networkUserId:guid}/roles")]
[ApiController]
public class NetworkUserRoleController(INetworkUserRoleService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadRole)]
    [RequirePermission(Constants.Permissions.ReadUser)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkUserRoleDto>))]
    public IActionResult GetNetworkUserRoles(Guid networkId, Guid networkUserId)
    {
        var networkUserRoles = service.GetByNetworkUserId(networkId, networkUserId);
        return Ok(mapper.Map<IEnumerable<NetworkUserRoleDto>>(networkUserRoles,
            opts => opts.Items["networkId"] = networkId));
    }

    [HttpGet("{roleId:guid}")]
    [RequirePermission(Constants.Permissions.ReadRole)]
    [RequirePermission(Constants.Permissions.ReadUser)]
    [ProducesResponseType(200, Type = typeof(NetworkUserRoleDto))]
    [ProducesResponseType(404)]
    public IActionResult GetNetworkUserRole(Guid networkId, Guid networkUserId, Guid roleId)
    {
        var networkUserRole = service.GetByIds(networkId, networkUserId, roleId);
        return networkUserRole != null
            ? Ok(mapper.Map<NetworkUserRoleDto>(networkUserRole, opts => opts.Items["networkId"] = networkId))
            : NotFound();
    }

    [HttpPost("{roleId:guid}")]
    [RequirePermission(Constants.Permissions.ManageRole)]
    [RequirePermission(Constants.Permissions.ManageUser)]
    [ProducesResponseType(200, Type = typeof(NetworkUserRoleDto))]
    public IActionResult CreateNetworkUserRole(Guid networkId, Guid networkUserId, Guid roleId,
        [FromBody] CreateNetworkUserRoleDto createNetworkUserRole)
    {
        var networkUserRole = service.Create(networkId, networkUserId, roleId, createNetworkUserRole);
        return Ok(mapper.Map<NetworkUserRoleDto>(networkUserRole, opts => opts.Items["networkId"] = networkId));
    }

    [HttpPut("{roleId:guid}")]
    [RequirePermission(Constants.Permissions.ManageRole)]
    [RequirePermission(Constants.Permissions.ManageUser)]
    [ProducesResponseType(200, Type = typeof(NetworkUserRoleDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateNetworkUserRole(Guid networkId, Guid networkUserId, Guid roleId,
        [FromBody] UpdateNetworkUserRoleDto updateNetworkUserRole)
    {
        var networkUserRole = service.Update(networkId, networkUserId, roleId, updateNetworkUserRole);
        return Ok(mapper.Map<NetworkUserRoleDto>(networkUserRole, opts => opts.Items["networkId"] = networkId));
    }

    [HttpDelete("{roleId:guid}")]
    [RequirePermission(Constants.Permissions.ManageRole)]
    [RequirePermission(Constants.Permissions.ManageUser)]
    [ProducesResponseType(204)]
    public IActionResult DeleteNetworkUserRole(Guid networkId, Guid networkUserId, Guid roleId)
    {
        service.DeleteByIds(networkId, networkUserId, roleId);
        return NoContent();
    }
}