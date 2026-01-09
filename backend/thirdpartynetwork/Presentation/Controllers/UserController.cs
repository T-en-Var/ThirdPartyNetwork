﻿using AutoMapper;
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
    public async Task<IActionResult> GetAll()
    {
        var users = await service.GetAllAsync();
        return StatusCode(200, mapper.Map<IEnumerable<UserDto>>(users));
    }
    
    [HttpGet("{userId:guid}")]
    [RequirePermission(Constants.Permissions.ReadUser)]
    [ProducesResponseType(200, Type = typeof(UserDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get(Guid userId)
    {
        var user = await service.GetByIdAsync(userId);
        return user != null ? StatusCode(200, mapper.Map<UserDto>(user)) : NotFound();
    }
    
    [HttpPost]
    [RequirePermission(Constants.Permissions.ManageUser)]
    [ProducesResponseType(201, Type = typeof(UserDto))]
    public async Task<IActionResult> Create([FromBody] CreateUserDto createUserDto)
    {
        var user = await service.CreateAsync(createUserDto);
        return StatusCode(200, mapper.Map<UserDto>(user));
    }
    
    [HttpPut("{userId:guid}")]
    [RequirePermission(Constants.Permissions.ManageUser)]
    [ProducesResponseType(201, Type = typeof(UserDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid userId, [FromBody] UpdateUserDto updateUserDto)
    {
        var user = await service.UpdateAsync(userId, updateUserDto);
        return StatusCode(200, mapper.Map<UserDto>(user));
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
