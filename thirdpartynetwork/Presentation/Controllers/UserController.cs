using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("users")]
[ApiController]
public class UserController(IUserService service, INetworkService networkService, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Constants.Permissions.ReadUser)]
    [ProducesResponseType(200, Type = typeof(List<UserWithNetworksDto>))]
    public async Task<IActionResult> GetAll()
    {
        var users = await service.GetAllAsync();
        var mainNetwork = await networkService.GetMainNetworkAsync();
        return StatusCode(200,
            mapper.Map<List<UserWithNetworksDto>>(users, opts => { opts.Items["networkId"] = mainNetwork.Id; }));
    }

    [HttpGet("{userId:guid}")]
    [RequirePermission(Constants.Permissions.ReadUser)]
    [ProducesResponseType(200, Type = typeof(UserDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get(Guid userId)
    {
        var user = await service.GetByIdAsync(userId);
        var mainNetwork = await networkService.GetMainNetworkAsync();
        return user != null
            ? StatusCode(200, mapper.Map<UserDto>(user, opts => { opts.Items["networkId"] = mainNetwork.Id; }))
            : NotFound();
    }

    [HttpPost]
    [RequirePermission(Constants.Permissions.ManageUser)]
    [ProducesResponseType(201, Type = typeof(UserDto))]
    public async Task<IActionResult> Create([FromBody] CreateUserDto createUserDto)
    {
        var user = await service.CreateAsync(createUserDto);
        var mainNetwork = await networkService.GetMainNetworkAsync();
        return StatusCode(200, mapper.Map<UserDto>(user, opts => { opts.Items["networkId"] = mainNetwork.Id; }));
    }

    [HttpPut("{userId:guid}")]
    [RequirePermission(Constants.Permissions.ManageUser)]
    [ProducesResponseType(201, Type = typeof(UserDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid userId, [FromBody] UpdateUserDto updateUserDto)
    {
        var user = await service.UpdateAsync(userId, updateUserDto);
        var mainNetwork = await networkService.GetMainNetworkAsync();
        return StatusCode(200, mapper.Map<UserDto>(user, opts => { opts.Items["networkId"] = mainNetwork.Id; }));
    }

    [HttpDelete("{userId:guid}")]
    [RequirePermission(Constants.Permissions.ManageUser)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(Guid userId)
    {
        await service.DeleteByIdAsync(userId);
        return NoContent();
    }
}