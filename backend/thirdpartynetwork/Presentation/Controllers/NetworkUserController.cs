using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks/{networkId:guid}/users")]
[ApiController]
public class NetworkUserController(INetworkUserService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadUser, Constants.Permissions.IsNetworkUserOwner)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkUserDto>))]
    public async Task<IActionResult> GetNetworkUsers(Guid networkId)
    {
        var networkUsers = await service.GetByNetworkIdAsync(networkId);
        return Ok(mapper.Map<IEnumerable<NetworkUserDto>>(networkUsers, opts => opts.Items["networkId"] = networkId));
    }

    [HttpGet("{networkUserId:guid}")]
    [RequirePermission(Constants.Permissions.ManageUser, Constants.Permissions.IsNetworkUserOwner)]
    [ProducesResponseType(200, Type = typeof(NetworkUserDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetNetworkUser(Guid networkId, Guid networkUserId)
    {
        var networkUser = await service.GetByIdAsync(networkId, networkUserId);
        return networkUser != null
            ? Ok(mapper.Map<NetworkUserDto>(networkUser, opts => opts.Items["networkId"] = networkId))
            : NotFound();
    }

    [HttpPost("{userProxyId:guid}")]
    [RequirePermission(Constants.Permissions.ManageUser, Constants.Permissions.IsUserProxyOwner)]
    [ProducesResponseType(201, Type = typeof(NetworkUser))]
    public async Task<IActionResult> CreateNetworkUser(Guid networkId, Guid userProxyId,
        [FromBody] CreateNetworkUserDto networkUser)
    {
        var user = await service.CreateAsync(networkId, userProxyId, networkUser);
        return Ok(mapper.Map<NetworkUserDto>(user, opts => opts.Items["networkId"] = networkId));
    }

    [HttpPut("{networkUserId:guid}")]
    [RequirePermission(Constants.Permissions.ManageUser)]
    [ProducesResponseType(200, Type = typeof(NetworkUserDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateNetworkUser(Guid networkId, Guid networkUserId,
        [FromBody] UpdateNetworkUserDto updateNetworkUser)
    {
        var networkUser = await service.UpdateAsync(networkId, networkUserId, updateNetworkUser);
        return Ok(mapper.Map<NetworkUserDto>(networkUser, opts => opts.Items["networkId"] = networkId));
    }

    [HttpDelete("{networkUserId:guid}")]
    [RequirePermission(Constants.Permissions.ManageUser, Constants.Permissions.IsNetworkUserOwner)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteNetworkUser(Guid networkId, Guid networkUserId)
    {
        await service.DeleteByIdAsync(networkId, networkUserId);
        return NoContent();
    }
}