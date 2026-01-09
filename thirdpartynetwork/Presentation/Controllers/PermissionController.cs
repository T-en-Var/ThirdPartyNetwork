using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("permissions")]
[ApiController]
public class PermissionController(IPermissionService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadPermission)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<PermissionDto>))]
    public async Task<IActionResult> GetPermissions()
    {
        var permissions = await service.GetAllAsync();
        return Ok(mapper.Map<IEnumerable<PermissionDto>>(permissions));
    }

    [HttpGet("{permissionId:guid}")]
    [RequirePermission(Constants.Permissions.ReadPermission)]
    [ProducesResponseType(200, Type = typeof(PermissionDto))]
    public async Task<IActionResult> GetPermission(Guid permissionId)
    {
        var permission = await service.GetByIdAsync(permissionId);
        return permission != null ? Ok(mapper.Map<PermissionDto>(permission)) : NotFound();
    }

    [HttpPost]
    [RequirePermission(Constants.Permissions.SystemManaged)]
    [ProducesResponseType(201, Type = typeof(PermissionDto))]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionDto createPermission)
    {
        var permission = await service.CreateAsync(createPermission);
        return Ok(mapper.Map<PermissionDto>(permission));
    }

    [HttpPut("{permissionId:guid}")]
    [RequirePermission(Constants.Permissions.ManagePermission)]
    [ProducesResponseType(200, Type = typeof(PermissionDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdatePermission(Guid permissionId, [FromBody] UpdatePermissionDto updatePermission)
    {
        var permission = await service.UpdateAsync(permissionId, updatePermission);
        return Ok(mapper.Map<PermissionDto>(permission));
    }

    [HttpDelete("{permissionId:guid}")]
    [RequirePermission(Constants.Permissions.SystemManaged)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeletePermission(Guid permissionId)
    {
        await service.DeleteByIdAsync(permissionId);
        return NoContent();
    }
}
