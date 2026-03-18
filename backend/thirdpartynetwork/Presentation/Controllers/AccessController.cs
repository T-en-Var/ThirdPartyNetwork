﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("accesses")]
[ApiController]
public class AccessController(IAccessService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    // [RequirePermission(Constants.Permissions.ReadAccess)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkDto>))]
    public async Task<IActionResult> GetAccesses()
    {
        var accesses = await service.GetAllAsync();
        return Ok(mapper.Map<IEnumerable<AccessDto>>(accesses));
    }

    [HttpGet("{accessId:guid}")]
    // [RequirePermission(Constants.Permissions.ManageAccess)]
    [ProducesResponseType(200, Type = typeof(AccessDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAccess(Guid accessId)
    {
        var access = await service.GetByIdAsync(accessId);
        return Ok(mapper.Map<AccessDto>(access));
    }

    [HttpPost]
    [RequirePermission(Constants.Permissions.ManageAccess)]
    [ProducesResponseType(201, Type = typeof(NetworkAccess))]
    public async Task<IActionResult> CreateAccess([FromBody] CreateAccessDto createAccess)
    {
        var access = await service.CreateAsync(createAccess);
        return StatusCode(201, mapper.Map<AccessDto>(access));
    }

    [HttpPut("{accessId:guid}")]
    [RequirePermission(Constants.Permissions.ManageAccess)]
    [ProducesResponseType(200, Type = typeof(AccessDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateAccess(Guid accessId, [FromBody] UpdateAccessDto updateAccess)
    {
        var access = await service.UpdateAsync(accessId, updateAccess);
        return Ok(mapper.Map<AccessDto>(access));
    }

    [HttpDelete("{accessId:guid}")]
    [RequirePermission(Constants.Permissions.ManageAccess)]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeletePermission(Guid accessId)
    {
        await service.DeleteByIdAsync(accessId);
        return NoContent();
    }
}
