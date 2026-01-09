using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks/{networkId:guid}/roles/{roleId:guid}/permissions")]
[ApiController]
public class RolePermissionController(IRolePermissionService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadRole)]
    [RequirePermission(Constants.Permissions.ReadPermission)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<RolePermissionDto>))]
    public async Task<IActionResult> GetRolePermissions(Guid networkId, Guid roleId)
    {
        var rolePermissions = await service.GetByRoleIdAsync(networkId, roleId);
        return Ok(mapper.Map<IEnumerable<RolePermissionDto>>(rolePermissions));
    }

    [HttpGet("{permissionId:guid}")]
    [RequirePermission(Constants.Permissions.ReadRole)]
    [RequirePermission(Constants.Permissions.ReadPermission)]
    [ProducesResponseType(200, Type = typeof(RolePermissionDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetRolePermission(Guid networkId, Guid roleId, Guid permissionId)
    {
        var rolePermission = await service.GetByIdsAsync(networkId, roleId, permissionId);
        return rolePermission != null ? Ok(mapper.Map<RolePermissionDto>(rolePermission)) : NotFound();
    }

    [HttpPost("{permissionId:guid}")]
    [RequirePermission(Constants.Permissions.ManageRole)]
    [RequirePermission(Constants.Permissions.ManagePermission)]
    [ProducesResponseType(201, Type = typeof(RolePermissionDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateRolePermission(Guid networkId, Guid roleId, Guid permissionId,
        [FromBody] CreateRolePermissionDto createRolePermission)
    {
        var rolePermission = await service.CreateAsync(networkId, roleId, permissionId, createRolePermission);
        return Ok(mapper.Map<RolePermissionDto>(rolePermission));
    }

    [HttpPut("{permissionId:guid}")]
    [RequirePermission(Constants.Permissions.ManageRole)]
    [RequirePermission(Constants.Permissions.ManagePermission)]
    [ProducesResponseType(200, Type = typeof(RolePermissionDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateRolePermission(Guid networkId, Guid roleId, Guid permissionId,
        [FromBody] UpdateRolePermissionDto updateRolePermission)
    {
        var rolePermission = await service.UpdateAsync(networkId, roleId, permissionId, updateRolePermission);
        return Ok(mapper.Map<RolePermissionDto>(rolePermission));
    }

    [HttpDelete("{permissionId:guid}")]
    [RequirePermission(Constants.Permissions.ManageRole)]
    [RequirePermission(Constants.Permissions.ManagePermission)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteRolePermission(Guid networkId, Guid roleId, Guid permissionId)
    {
        await service.DeleteByIdsAsync(networkId, roleId, permissionId);
        return NoContent();
    }
}