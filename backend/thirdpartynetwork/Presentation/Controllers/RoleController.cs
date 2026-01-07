using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks/{networkId:guid}/roles/")]
[ApiController]
public class RoleController(IRoleService service, IMapper mapper) : Controller
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadRole)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<RoleDto>))]
    public IActionResult GetRoles(Guid networkId)
    {
        var roles = service.GetByNetworkId(networkId);
        return Ok(mapper.Map<IEnumerable<RoleDto>>(roles));
    }

    [HttpGet("{roleId:guid}")]
    [RequirePermission(Constants.Permissions.ReadRole)]
    [ProducesResponseType(200, Type = typeof(RoleDto))]
    [ProducesResponseType(404)]
    public IActionResult GetRole(Guid networkId, Guid roleId)
    {
        var role = service.GetById(networkId, roleId);
        return role != null ? Ok(mapper.Map<RoleDto>(role)) : NotFound();
    }

    [HttpPost]
    [RequirePermission(Constants.Permissions.ManageRole)]
    [ProducesResponseType(201, Type = typeof(RoleDto))]
    public IActionResult CreateRole(Guid networkId, [FromBody] CreateRoleDto createRole)
    {
        var role = service.Create(networkId, createRole);
        return Ok(mapper.Map<RoleDto>(role));
    }

    [HttpPut("{roleId:guid}")]
    [RequirePermission(Constants.Permissions.ManageRole)]
    [ProducesResponseType(200, Type = typeof(RoleDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateRole(Guid networkId, Guid roleId, [FromBody] UpdateRoleDto updateRole)
    {
        var role = service.Update(networkId, roleId, updateRole);
        return Ok(mapper.Map<RoleDto>(role));
    }

    [HttpDelete("{roleId:guid}")]
    [RequirePermission(Constants.Permissions.ManageRole)]
    [ProducesResponseType(204)]
    public IActionResult DeleteRole(Guid networkId, Guid roleId)
    {
        service.DeleteById(networkId, roleId);
        return NoContent();
    }
}