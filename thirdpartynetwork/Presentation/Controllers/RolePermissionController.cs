using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/networks/{networkId:guid}/roles/{roleId:guid}/permissions")]
[ApiController]
public class RolePermissionController(IRolePermissionService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<RolePermissionDto>))]
    public IActionResult GetRolePermissions(Guid networkId, Guid roleId)
    {
        var rolePermissions = service.GetByRoleId(networkId, roleId);
        return Ok(mapper.Map<IEnumerable<RolePermissionDto>>(rolePermissions));
    }

    [HttpGet("{permissionId:guid}")]
    [ProducesResponseType(200, Type = typeof(RolePermissionDto))]
    [ProducesResponseType(404)]
    public IActionResult GetRolePermission(Guid networkId, Guid roleId, Guid permissionId)
    {
        var rolePermission = service.GetByIds(networkId, roleId, permissionId);
        return rolePermission != null ? Ok(mapper.Map<RolePermissionDto>(rolePermission)) : NotFound();
    }

    [HttpPost("{permissionId:guid}")]
    [ProducesResponseType(201, Type = typeof(RolePermissionDto))]
    [ProducesResponseType(404)]
    public IActionResult CreateRolePermission(Guid networkId, Guid roleId, Guid permissionId,
        CreateRolePermissionDto createRolePermission)
    {
        var rolePermission = service.Create(networkId, roleId, permissionId, createRolePermission);
        return Ok(mapper.Map<RolePermissionDto>(rolePermission));
    }

    [HttpPut("{permissionId:guid}")]
    [ProducesResponseType(200, Type = typeof(RolePermissionDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateRolePermission(Guid networkId, Guid roleId, Guid permissionId, UpdateRolePermissionDto updateRolePermission)
    {
        var rolePermission = service.Update(networkId, roleId, permissionId, updateRolePermission);
        return Ok(mapper.Map<RolePermissionDto>(rolePermission));
    }

    [HttpDelete("{permissionId:guid}")]
    [ProducesResponseType(204)]
    public IActionResult DeleteRolePermission(Guid networkId, Guid roleId, Guid permissionId)
    {
        service.DeleteByIds(networkId, roleId, permissionId);
        return NoContent();
    }
}