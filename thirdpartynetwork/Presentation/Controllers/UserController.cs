using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("users")]
[ApiController]
public class UserController(IUserService service, IMapper mapper) : ControllerBase
{
    // ClaimsPrincipal User = HttpContext.User;
    
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadUser)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<UserDto>))]
    public IActionResult GetAll()
    {
        var users = service.GetAll();
        return StatusCode(200, mapper.Map<IEnumerable<UserDto>>(users));
    }
    
    [HttpGet("{userId:guid}")]
    [RequirePermission(Constants.Permissions.ReadUser)]
    [ProducesResponseType(200, Type = typeof(UserDto))]
    [ProducesResponseType(404)]
    public IActionResult Get(Guid userId)
    {
        var user = service.GetById(userId);
        return user != null ? StatusCode(200, mapper.Map<UserDto>(user)) : NotFound();
    }
    
    [HttpPost]
    [RequirePermission(Constants.Permissions.ManageUser)]
    [ProducesResponseType(201, Type = typeof(UserDto))]
    public IActionResult Create([FromBody] CreateUserDto createUserDto)
    {
        var user = service.Create(createUserDto);
        return StatusCode(200, mapper.Map<UserDto>(user));
    }
    
    [HttpPut("{userId:guid}")]
    [RequirePermission(Constants.Permissions.ManageUser)]
    [ProducesResponseType(201, Type = typeof(UserDto))]
    [ProducesResponseType(404)]
    public IActionResult Update(Guid userId, [FromBody] UpdateUserDto updateUserDto)
    {
        var user = service.Update(userId, updateUserDto);
        return StatusCode(200, mapper.Map<UserDto>(user));
    }
    
    [HttpDelete("{userId:guid}")]
    [RequirePermission(Constants.Permissions.ManageUser)]
    [ProducesResponseType(204)]
    public IActionResult Delete(Guid userId)
    {
        service.DeleteById(userId);
        return NoContent();
    }
}
