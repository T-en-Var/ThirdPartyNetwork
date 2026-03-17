using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("metrics")]
[ApiController]
public class MetricsController(INetworkService networkService, IUserService userService) : ControllerBase
{
    [HttpGet("networks")]
    [Authorize]
    [RequirePermission(Constants.Permissions.ManageNetwork)]
    [ProducesResponseType(200, Type = typeof(ICollection<NetworkMetricsDto>))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetNetworksMetrics()
    {
        var result = await networkService.GetAllWithMetricsAsync();
        return StatusCode(200, result);
    }
    
    [HttpGet("networks/{networkId:guid}")]
    [Authorize]
    [RequirePermission(Constants.Permissions.ManageNetwork)]
    [ProducesResponseType(200, Type = typeof(NetworkMetricsDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetNetworkMetrics(Guid networkId)
    {
        var result = await networkService.GetByIdWithMetricsAsync(networkId);
        
        return StatusCode(200, result);
    }
    
    [HttpGet("users")]
    [Authorize]
    [RequirePermission(Constants.Permissions.ManageNetwork)]
    [ProducesResponseType(200, Type = typeof(ICollection<UserMetricsDto>))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUsersMetrics()
    {
        var result = await userService.GetAllWithMetricsAsync();
        return StatusCode(200, result);
    }
    
    [HttpGet("users/{userId:guid}")]
    [Authorize]
    [RequirePermission(Constants.Permissions.ManageNetwork)]
    [ProducesResponseType(200, Type = typeof(UserMetricsDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserMetrics(Guid userId)
    {
        var result = await userService.GetByIdWithMetricsAsync(userId);
        return StatusCode(200, result);
    }
}