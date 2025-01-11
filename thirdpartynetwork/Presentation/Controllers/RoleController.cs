using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/roles")]
[ApiController]
public class RoleController(IRoleService service, IMapper mapper) : Controller
{
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<RoleDto>))]
    public IActionResult GetRoles()
    {
        var roles = service.GetAll();
        return Ok(mapper.Map<IEnumerable<RoleDto>>(roles));
    }

    [HttpGet("{roleId:guid}")]
    [ProducesResponseType(200, Type = typeof(RoleDto))]
    [ProducesResponseType(404)]
    public IActionResult GetRole(Guid roleId)
    {
        var role = service.GetById(roleId);
        return role != null ? Ok(mapper.Map<RoleDto>(role)) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(201, Type = typeof(RoleDto))]
    public IActionResult CreateRole(CreateRoleDto createRole)
    {
        var role = service.Create(createRole);
        return Ok(mapper.Map<RoleDto>(role));
    }

    [HttpPatch("{roleId:guid}")]
    [ProducesResponseType(200, Type = typeof(RoleDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateRole(Guid roleId, UpdateRoleDto updateRole)
    {
        var role = service.Update(roleId, updateRole);
        return Ok(mapper.Map<RoleDto>(role));
    }

    [HttpDelete("{roleId:guid}")]
    [ProducesResponseType(204)]
    public IActionResult DeleteRole(Guid roleId)
    {
        service.DeleteById(roleId);
        return NoContent();
    }
}
