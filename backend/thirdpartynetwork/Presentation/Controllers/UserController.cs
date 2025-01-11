using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/users")]
[ApiController]
public class UserController(IUserService service, IMapper mapper) : ControllerBase
{
    // ClaimsPrincipal User = HttpContext.User;
    
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<UserDto>))]
    public IActionResult GetAll()
    {
        var users = service.GetAll();
        return StatusCode(200, mapper.Map<IEnumerable<UserDto>>(users));
    }
    
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(200, Type = typeof(UserDto))]
    [ProducesResponseType(404)]
    public IActionResult Get(Guid userId)
    {
        var user = service.GetById(userId);
        return user != null ? StatusCode(200, mapper.Map<UserDto>(user)) : NotFound();
    }
    
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(UserDto))]
    public IActionResult Create([FromBody] CreateUserProxyDto createUserProxyDto)
    {
        var user = service.Create(createUserProxyDto);
        return StatusCode(200, mapper.Map<UserDto>(user));
    }
    
    [HttpPatch("{userId:guid}")]
    [ProducesResponseType(201, Type = typeof(UserDto))]
    public IActionResult Update(Guid userId, [FromBody] UpdateUserProxyDto updateUserProxyDto)
    {
        var user = service.Update(userId, updateUserProxyDto);
        return StatusCode(200, mapper.Map<UserDto>(user));
    }
    
    [HttpDelete("{userId:guid}")]
    [ProducesResponseType(200, Type = typeof(UserDto))]
    [ProducesResponseType(404)]
    public IActionResult Delete(Guid userId)
    {
        service.DeleteById(userId);
        return NoContent();
    }
}
