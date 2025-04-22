using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/permissions")]
[ApiController]
public class PermissionController(IPermissionService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadPermission)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<PermissionDto>))]
    public IActionResult GetPermissions()
    {
        var permissions = service.GetAll();
        return Ok(mapper.Map<IEnumerable<PermissionDto>>(permissions));
    }

    [HttpGet("{permissionId:guid}")]
    [RequirePermission(Constants.Permissions.ReadPermission)]
    [ProducesResponseType(200, Type = typeof(PermissionDto))]
    public IActionResult GetPermission(Guid permissionId)
    {
        var permission = service.GetById(permissionId);
        return permission != null ? Ok(mapper.Map<PermissionDto>(permission)) : NotFound();
    }

    [HttpPost]
    [RequirePermission(Constants.Permissions.SystemManaged)]
    [ProducesResponseType(201, Type = typeof(PermissionDto))]
    public IActionResult CreatePermission([FromBody] CreatePermissionDto createPermission)
    {
        var permission = service.Create(createPermission);
        return Ok(mapper.Map<PermissionDto>(permission));
    }

    [HttpPut("{permissionId:guid}")]
    [RequirePermission(Constants.Permissions.ManagePermission)]
    [ProducesResponseType(200, Type = typeof(PermissionDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdatePermission(Guid permissionId, [FromBody] UpdatePermissionDto updatePermission)
    {
        var permission = service.Update(permissionId, updatePermission);
        return Ok(mapper.Map<PermissionDto>(permission));
    }

    [HttpDelete("{permissionId:guid}")]
    [RequirePermission(Constants.Permissions.SystemManaged)]
    [ProducesResponseType(204)]
    public IActionResult DeletePermission(Guid permissionId)
    {
        service.DeleteById(permissionId);
        return NoContent();
    }
}
