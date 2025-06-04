using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Core;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/networks")]
[ApiController]
public class NetworkController(INetworkService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkDto>))]
    public IActionResult GetNetworks()
    {
        var networks = service.GetAll();
        var networkDtos = networks
            .Select(network => mapper.Map<NetworkDto>(network, opts =>
            {
                opts.Items["networkId"] = network.Id;
            }))
            .ToList();
        return StatusCode(200, networkDtos);
    }

    [HttpGet("{networkId:guid}")]
    [RequirePermission(Constants.Permissions.ReadNetwork)]
    [ProducesResponseType(200, Type = typeof(NetworkDto))]
    [ProducesResponseType(404)]
    public IActionResult GetNetworkById(Guid networkId)
    {
        var network = service.GetById(networkId);
        return network != null ? Ok(mapper.Map<NetworkDto>(network, opts => opts.Items["networkId"] = networkId)) : NotFound();
    }

    [HttpGet("{networkId:guid}/details")]
    [ProducesResponseType(200, Type = typeof(NetworkDto))]
    [ProducesResponseType(404)]
    public IActionResult GetNetworkDetailsById(Guid networkId)
    {
        var network = service.GetDetailsById(networkId);
        return network != null ? Ok(mapper.Map<NetworkDto>(network, opts => opts.Items["networkId"] = networkId)) : NotFound();
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(201, Type = typeof(NetworkDto))]
    public IActionResult CreateNetwork([FromBody] CreateNetworkDto networkCreate)
    {
        var network = service.Create(networkCreate);
        return StatusCode(201, mapper.Map<NetworkDto>(network, opts => opts.Items["networkId"] = network.Id));
    }

    [HttpPut("{networkId:guid}")]
    [RequirePermission(Constants.Permissions.ManageNetwork)]
    [ProducesResponseType(200, Type = typeof(NetworkDto))]
    [ProducesResponseType(404)]
    public IActionResult Update(Guid networkId, UpdateNetworkDto networkUpdate)
    {
        var network = service.Update(networkId, networkUpdate);
        return StatusCode(200, mapper.Map<NetworkDto>(network, opts => opts.Items["networkId"] = networkId));
    }

    [HttpDelete("{networkId:guid}")]
    [RequirePermission(Constants.Permissions.ManageNetwork)]
    [ProducesResponseType(204)]
    public IActionResult Delete(Guid networkId)
    {
        service.DeleteById(networkId);
        return NoContent();
    }
}
