using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/networks")]
[ApiController]
public class NetworkController(INetworkService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    // [Authorize]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkDto>))]
    public IActionResult GetNetworks()
    {
        var networks = service.GetAll();
        return StatusCode(200, mapper.Map<IEnumerable<NetworkDto>>(networks));
    }

    [HttpGet("{networkId:guid}")]
    [ProducesResponseType(200, Type = typeof(NetworkDto))]
    [ProducesResponseType(404)]
    public IActionResult GetNetworkById(Guid networkId)
    {
        var network = service.GetById(networkId);
        return network != null ? Ok(mapper.Map<NetworkDto>(network)) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(201, Type = typeof(NetworkDto))]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public IActionResult CreateNetwork([FromBody] CreateNetworkDto networkCreate)
    {
        var network = service.Create(networkCreate);
        return StatusCode(201, mapper.Map<NetworkDto>(network));
    }

    [HttpPatch("{networkId:guid}")]
    public IActionResult Update(Guid networkId, UpdateNetworkDto networkUpdate)
    {
        var network = service.Update(networkId, networkUpdate);
        return StatusCode(200, mapper.Map<NetworkDto>(network));
    }

    [HttpDelete("{networkId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public IActionResult Delete(Guid networkId)
    {
        service.DeleteById(networkId);
        return NoContent();
    }
}
