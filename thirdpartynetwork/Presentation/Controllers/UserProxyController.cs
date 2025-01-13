using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/users/{userId:guid}/proxies")]
[ApiController]
public class UserProxyController(IUserProxyService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<UserProxyDto>))]
    public IActionResult GetUsers(Guid userId)
    {
        var userProxies = service.GetByUserId(userId);
        return Ok(mapper.Map<IEnumerable<UserProxyDto>>(userProxies));
    }

    [HttpGet("{userProxyId:guid}")]
    [ProducesResponseType(200, Type = typeof(UserProxyDto))]
    [ProducesResponseType(404)]
    public IActionResult GetUser(Guid userId, Guid userProxyId)
    {
        var userProxy = service.GetByIds(userId, userProxyId);
        return userProxy != null ? Ok(mapper.Map<UserProxyDto>(userProxy)) : NotFound();
    }
    
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(UserProxyDto))]
    [ProducesResponseType(404)]
    public IActionResult Create(Guid userId, CreateUserProxyDto createUserProxy)
    {
        var userProxy = service.Create(userId, createUserProxy);
        return Ok(mapper.Map<UserProxyDto>(userProxy));
    }

    [HttpPut("{userProxyId:guid}")]
    [ProducesResponseType(200, Type = typeof(UserProxyDto))]
    [ProducesResponseType(404)]
    public IActionResult Update(Guid userId, Guid userProxyId, UpdateUserProxyDto updateUserProxy)
    {
        var userProxy = service.Update(userId, userProxyId, updateUserProxy);
        return Ok(mapper.Map<UserProxyDto>(userProxy));
    }

    [HttpDelete("{userProxyId:guid}")]
    [ProducesResponseType(204)]
    public IActionResult Delete(Guid userId, Guid userProxyId)
    {
        service.DeleteByIds(userId, userProxyId);
        return NoContent();
    }
}
