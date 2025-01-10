using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/networks")]
[ApiController]
public class NetworkController(INetworkService service) : ControllerBase
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkDto>))]
    public IActionResult GetNetworks()
    {
        // ClaimsPrincipal User = HttpContext.User;
        var networks = service.GetAll();
        return StatusCode(200, networks);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(200, Type = typeof(NetworkDto))]
    [ProducesResponseType(404)]
    public IActionResult GetNetworkById(Guid id)
    {
        var network = service.GetById(id);
        return StatusCode(200, network);
    }

    [HttpPost]
    [ProducesResponseType(201, Type = typeof(NetworkDto))]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public IActionResult CreateNetwork([FromBody] CreateNetworkDto networkCreate)
    {
        var network = service.Create(networkCreate);
        return StatusCode(201, network);
    }

    [HttpPatch("/{id:guid}/")]
    public IActionResult Update(Guid id, UpdateNetworkDto networkUpdate)
    {
        var network = service.Update(id, networkUpdate);
        return StatusCode(200, network);
    }
}
