using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/networks/{networkId:guid}/users")]
[ApiController]
public class NetworkUserController(INetworkUserService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkUserDto>))]
    public IActionResult GetNetworkUsers(Guid networkId)
    {
        var networkUsers = service.GetByNetworkId(networkId);
        return Ok(mapper.Map<IEnumerable<NetworkUserDto>>(networkUsers));
    }

    [HttpGet("{networkUserId:guid}")]
    [ProducesResponseType(200, Type = typeof(NetworkUserDto))]
    [ProducesResponseType(404)]
    public IActionResult GetNetworkUser(Guid networkId, Guid networkUserId)
    {
        var networkUser = service.GetById(networkUserId);
        return networkUser != null ? Ok(mapper.Map<NetworkUserDto>(networkUser)) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(201, Type = typeof(NetworkUser))]
    public IActionResult CreateNetworkUser(Guid networkId, CreateNetworkUserDto networkUser)
    {
        var user = service.Create(networkId, networkUser);
        return Ok(mapper.Map<NetworkUserDto>(user));
    }

    [HttpPut("{networkUserId:guid}")]
    [ProducesResponseType(200, Type = typeof(NetworkUserDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateNetworkUser(Guid networkId, Guid networkUserId, UpdateNetworkUserDto updateNetworkUser)
    {
        var networkUser = service.Update(networkId, networkUserId, updateNetworkUser);
        return Ok(mapper.Map<NetworkUserDto>(networkUser));
    }
}