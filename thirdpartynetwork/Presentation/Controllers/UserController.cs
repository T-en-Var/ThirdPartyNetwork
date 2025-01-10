using Microsoft.AspNetCore.Mvc;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/users")]
[ApiController]
public class UserController(IUserProxyService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(IEnumerable<UserDto>))]
    public IActionResult Create([FromBody] CreateUserProxyDto createUserProxyDto)
    {
        var user = service.Create(createUserProxyDto);
        return StatusCode(200, user);
    }
}
