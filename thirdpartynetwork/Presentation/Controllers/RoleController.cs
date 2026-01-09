﻿using AutoMapper;
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
    public async Task<IActionResult> GetRoles(Guid networkId)
    {
        var roles = await service.GetByNetworkIdAsync(networkId);
        return Ok(mapper.Map<IEnumerable<RoleDto>>(roles));
    }

    [HttpGet("{roleId:guid}")]
    [RequirePermission(Constants.Permissions.ReadRole)]
    [ProducesResponseType(200, Type = typeof(RoleDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetRole(Guid networkId, Guid roleId)
    {
        var role = await service.GetByIdAsync(networkId, roleId);
        return role != null ? Ok(mapper.Map<RoleDto>(role)) : NotFound();
    }

    [HttpPost]
    [RequirePermission(Constants.Permissions.ManageRole)]
    [ProducesResponseType(201, Type = typeof(RoleDto))]
    public async Task<IActionResult> CreateRole(Guid networkId, [FromBody] CreateRoleDto createRole)
    {
        var role = await service.CreateAsync(networkId, createRole);
        return Ok(mapper.Map<RoleDto>(role));
    }

    [HttpPut("{roleId:guid}")]
    [RequirePermission(Constants.Permissions.ManageRole)]
    [ProducesResponseType(200, Type = typeof(RoleDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateRole(Guid networkId, Guid roleId, [FromBody] UpdateRoleDto updateRole)
    {
        var role = await service.UpdateAsync(networkId, roleId, updateRole);
        return Ok(mapper.Map<RoleDto>(role));
    }

    [HttpDelete("{roleId:guid}")]
    [RequirePermission(Constants.Permissions.ManageRole)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteRole(Guid networkId, Guid roleId)
    {
        await service.DeleteByIdAsync(networkId, roleId);
        return NoContent();
    }
}