using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/networks/{networkId:guid}/users")]
[ApiController]
public class NetworkUserController(INetworkUserService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadUser)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkUserDto>))]
    public IActionResult GetNetworkUsers(Guid networkId)
    {
        var networkUsers = service.GetByNetworkId(networkId);
        return Ok(mapper.Map<IEnumerable<NetworkUserDto>>(networkUsers));
    }

    [HttpGet("{networkUserId:guid}")]
    [RequirePermission(Constants.Permissions.ManageUser)]
    [ProducesResponseType(200, Type = typeof(NetworkUserDto))]
    [ProducesResponseType(404)]
    public IActionResult GetNetworkUser(Guid networkId, Guid networkUserId)
    {
        var networkUser = service.GetById(networkId, networkUserId);
        return networkUser != null ? Ok(mapper.Map<NetworkUserDto>(networkUser)) : NotFound();
    }

    [HttpPost("{userProxyId:guid}")]
    [RequirePermission(Constants.Permissions.ManageUser)]
    [ProducesResponseType(201, Type = typeof(NetworkUser))]
    public IActionResult CreateNetworkUser(Guid networkId, Guid userProxyId, CreateNetworkUserDto networkUser)
    {
        var user = service.Create(networkId, userProxyId, networkUser);
        return Ok(mapper.Map<NetworkUserDto>(user));
    }

    [HttpPut("{networkUserId:guid}")]
    [RequirePermission(Constants.Permissions.ManageUser)]
    [ProducesResponseType(200, Type = typeof(NetworkUserDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateNetworkUser(Guid networkId, Guid networkUserId, UpdateNetworkUserDto updateNetworkUser)
    {
        var networkUser = service.Update(networkId, networkUserId, updateNetworkUser);
        return Ok(mapper.Map<NetworkUserDto>(networkUser));
    }

    [HttpDelete("{networkUserId:guid}")]
    [RequirePermission(Constants.Permissions.ManageUser)]
    [ProducesResponseType(204)]
    public IActionResult DeleteNetworkUser(Guid networkId, Guid networkUserId)
    {
        service.DeleteById(networkId, networkUserId);
        return NoContent();
    }
}
