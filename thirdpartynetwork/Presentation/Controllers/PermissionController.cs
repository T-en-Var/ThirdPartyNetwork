using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/permissions")]
[ApiController]
public class PermissionController(IPermissionService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkDto>))]
    public IActionResult GetPermissions()
    {
        var permissions = service.GetAll();
        return Ok(mapper.Map<IEnumerable<PermissionDto>>(permissions));
    }

    [HttpGet("{permissionId:guid}")]
    [ProducesResponseType(200, Type = typeof(PermissionDto))]
    [ProducesResponseType(404)]
    public IActionResult GetPermission(Guid permissionId)
    {
        var permission = service.GetById(permissionId);
        return Ok(mapper.Map<PermissionDto>(permission));
    }

    [HttpPost]
    [ProducesResponseType(201, Type = typeof(NetworkPermission))]
    public IActionResult CreatePermission([FromBody] CreatePermissionDto createPermission)
    {
        var permission = service.Create(createPermission);
        return StatusCode(201, mapper.Map<NetworkPermission>(permission));
    }

    [HttpPatch("{permissionId:guid}")]
    [ProducesResponseType(200, Type = typeof(PermissionDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdatePermission(Guid permissionId, [FromBody] UpdatePermissionDto updatePermission)
    {
        var permission = service.Update(permissionId, updatePermission);
        return Ok(mapper.Map<PermissionDto>(permission));
    }

    [HttpDelete("{permissionId:guid}")]
    [ProducesResponseType(204)]
    public IActionResult DeletePermission(Guid permissionId)
    {
        service.DeleteById(permissionId);
        return NoContent();
    }
}
