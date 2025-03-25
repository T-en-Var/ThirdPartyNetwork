using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/accesses")]
[ApiController]
public class AccessController(IAccessService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.EditNetworkAccesses)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkDto>))]
    public IActionResult GetAccesses()
    {
        var accesses = service.GetAll();
        return Ok(mapper.Map<IEnumerable<AccessDto>>(accesses));
    }

    [HttpGet("{accessId:guid}")]
    [RequirePermission(Constants.Permissions.EditNetworkAccesses)]
    [ProducesResponseType(200, Type = typeof(AccessDto))]
    [ProducesResponseType(404)]
    public IActionResult GetAccess(Guid accessId)
    {
        var access = service.GetById(accessId);
        return Ok(mapper.Map<AccessDto>(access));
    }

    [HttpPost]
    [RequirePermission(Constants.Permissions.EditNetworkAccesses)]
    [ProducesResponseType(201, Type = typeof(NetworkAccess))]
    public IActionResult CreateAccess([FromBody] CreateAccessDto createAccess)
    {
        var access = service.Create(createAccess);
        return StatusCode(201, mapper.Map<AccessDto>(access));
    }

    [HttpPut("{accessId:guid}")]
    [RequirePermission(Constants.Permissions.EditNetworkAccesses)]
    [ProducesResponseType(200, Type = typeof(AccessDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateAccess(Guid accessId, [FromBody] UpdateAccessDto updateAccess)
    {
        var access = service.Update(accessId, updateAccess);
        return Ok(mapper.Map<AccessDto>(access));
    }

    [HttpDelete("{accessId:guid}")]
    [RequirePermission(Constants.Permissions.EditNetworkAccesses)]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public IActionResult DeletePermission(Guid accessId)
    {
        service.DeleteById(accessId);
        return NoContent();
    }
}
