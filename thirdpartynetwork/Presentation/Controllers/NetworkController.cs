using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Dtos;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/networks")]
[ApiController]
public class NetworkController : ControllerBase
{
    private readonly INetworkService _service;

    public NetworkController(INetworkService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(200, Type = typeof(IEnumerable<NetworkDto>))]
    public IActionResult GetNetworks()
    {
        ICollection<NetworkDto> networks = _service.GetAll();
        return StatusCode(200, networks);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(200, Type = typeof(NetworkDto))]
    [ProducesResponseType(404)]
    public IActionResult GetNetworkById(Guid id)
    {
        NetworkDto? network = _service.GetById(id);
        return StatusCode(200, network);
    }

    [HttpPost]
    [ProducesResponseType(201, Type = typeof(NetworkDto))]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public IActionResult CreateNetwork([FromBody] CreateNetworkDto networkCreate)
    {
        NetworkDto? network = _service.Create(networkCreate);
        return StatusCode(201, network);
    }

    [HttpPatch("/{id}/")]
    public IActionResult Update(Guid id, UpdateNetworkDto networkUpdate)
    {
        NetworkDto? network = _service.Update(id, networkUpdate);
        return StatusCode(200, network);
    }
}
