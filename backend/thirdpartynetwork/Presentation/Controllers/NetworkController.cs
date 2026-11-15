﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("networks")]
[ApiController]
public class NetworkController(INetworkService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkDto>))]
    public async Task<IActionResult> GetNetworks()
    {
        var networks = await service.GetAllAsync();
        var networkDtos = networks
            .Select(network => mapper.Map<NetworkDto>(network, opts => { opts.Items["networkId"] = network.Id; }))
            .ToList();
        return StatusCode(200, networkDtos);
    }

    [HttpGet("{networkId:guid}")]
    [RequirePermission(Constants.Permissions.ReadNetwork)]
    [ProducesResponseType(200, Type = typeof(NetworkDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetNetworkById(Guid networkId)
    {
        var network = await service.GetByIdAsync(networkId);
        return network != null
            ? Ok(mapper.Map<NetworkDto>(network, opts => opts.Items["networkId"] = networkId))
            : NotFound();
    }

    [HttpGet("{networkId:guid}/details")]
    [ProducesResponseType(200, Type = typeof(NetworkDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetNetworkDetailsById(Guid networkId)
    {
        var network = await service.GetDetailsByIdAsync(networkId);
        return network != null
            ? Ok(mapper.Map<NetworkDto>(network, opts => opts.Items["networkId"] = networkId))
            : NotFound();
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(201, Type = typeof(NetworkDto))]
    public async Task<IActionResult> CreateNetwork([FromBody] CreateNetworkDto networkCreate)
    {
        var network = await service.CreateAsync(networkCreate);
        return StatusCode(201, mapper.Map<NetworkDto>(network, opts => opts.Items["networkId"] = network.Id));
    }

    [HttpPut("{networkId:guid}")]
    [RequirePermission(Constants.Permissions.ManageNetwork)]
    [ProducesResponseType(200, Type = typeof(NetworkDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid networkId, [FromBody] UpdateNetworkDto networkUpdate)
    {
        var network = await service.UpdateAsync(networkId, networkUpdate);
        return StatusCode(200, mapper.Map<NetworkDto>(network, opts => opts.Items["networkId"] = networkId));
    }

    [HttpDelete("{networkId:guid}")]
    [RequirePermission(Constants.Permissions.ManageNetwork)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(Guid networkId)
    {
        await service.DeleteByIdAsync(networkId);
        return NoContent();
    }
}