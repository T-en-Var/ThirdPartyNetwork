using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/networks/{networkId:guid}/roles/")]
[ApiController]
public class RoleController(IRoleService service, IMapper mapper) : Controller
{
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<RoleDto>))]
    public IActionResult GetRoles(Guid networkId)
    {
        var roles = service.GetByNetworkId(networkId);
        return Ok(mapper.Map<IEnumerable<RoleDto>>(roles));
    }

    [HttpGet("{roleId:guid}")]
    [ProducesResponseType(200, Type = typeof(RoleDto))]
    [ProducesResponseType(404)]
    public IActionResult GetRole(Guid networkId, Guid roleId)
    {
        var role = service.GetById(networkId, roleId);
        return role != null ? Ok(mapper.Map<RoleDto>(role)) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(201, Type = typeof(RoleDto))]
    public IActionResult CreateRole(Guid networkId, CreateRoleDto createRole)
    {
        var role = service.Create(networkId, createRole);
        return Ok(mapper.Map<RoleDto>(role));
    }

    [HttpPut("{roleId:guid}")]
    [ProducesResponseType(200, Type = typeof(RoleDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateRole(Guid networkId, Guid roleId, UpdateRoleDto updateRole)
    {
        var role = service.Update(networkId, roleId, updateRole);
        return Ok(mapper.Map<RoleDto>(role));
    }

    [HttpDelete("{roleId:guid}")]
    [ProducesResponseType(204)]
    public IActionResult DeleteRole(Guid networkId, Guid roleId)
    {
        service.DeleteById(networkId, roleId);
        return NoContent();
    }
}
