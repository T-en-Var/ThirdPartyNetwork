using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/users/{userId:guid}/proxies")]
[ApiController]
public class UserProxyController(IUserProxyService service, IMapper mapper, INetworkUserService networkUserService) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadUser)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<UserProxyDto>))]
    public IActionResult GetProxies(Guid userId)
    {
        var userProxies = service.GetByUserId(userId);
        return Ok(mapper.Map<IEnumerable<UserProxyDto>>(userProxies));
    }

    [HttpGet("{userProxyId:guid}")]
    [RequirePermission(Constants.Permissions.ReadUser, Constants.Permissions.IsUserProxyOwner)]
    [ProducesResponseType(200, Type = typeof(UserProxyDto))]
    [ProducesResponseType(404)]
    public IActionResult GetProxy(Guid userId, Guid userProxyId)
    {
        var userProxy = service.GetByIds(userId, userProxyId);
        return userProxy != null ? Ok(mapper.Map<UserProxyDto>(userProxy)) : NotFound();
    }
    
    [HttpPost]
    [RequirePermission(Constants.Permissions.IsUserProxyOwner)]
    [ProducesResponseType(201, Type = typeof(UserProxyDto))]
    [ProducesResponseType(404)]
    public IActionResult Create(Guid userId, CreateUserProxyDto createUserProxy)
    {
        var userProxy = service.Create(userId, createUserProxy);
        return Ok(mapper.Map<UserProxyDto>(userProxy));
    }

    [HttpPut("{userProxyId:guid}")]
    [RequirePermission(Constants.Permissions.IsUserProxyOwner)]
    [ProducesResponseType(200, Type = typeof(UserProxyDto))]
    [ProducesResponseType(404)]
    public IActionResult Update(Guid userId, Guid userProxyId, UpdateUserProxyDto updateUserProxy)
    {
        var userProxy = service.Update(userId, userProxyId, updateUserProxy);
        return Ok(mapper.Map<UserProxyDto>(userProxy));
    }

    [HttpDelete("{userProxyId:guid}")]
    [RequirePermission(Constants.Permissions.IsUserProxyOwner)]
    [ProducesResponseType(204)]
    public IActionResult Delete(Guid userId, Guid userProxyId)
    {
        service.DeleteByIds(userId, userProxyId);
        return NoContent();
    }

    [HttpGet("{userProxyId:guid}/networks")]
    [RequirePermission(Constants.Permissions.ReadUser, Constants.Permissions.IsUserProxyOwner)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkDto>))]
    public IActionResult GetNetworks(Guid userId, Guid userProxyId)
    {
        var networks = networkUserService.GetNetworksByUserProxyId(userProxyId);
        var networkDtos = networks
            .Select(network => mapper.Map<NetworkDto>(network, opts =>
            {
                opts.Items["networkId"] = network.Id;
            }))
            .ToList();
        return StatusCode(200, networkDtos);
    }
}
